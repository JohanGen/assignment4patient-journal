using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PatientJournal.API.Data;
using PatientJournal.API.Hubs;
using PatientJournal.Shared.DTOs;
using PatientJournal.Shared.Models;
using PatientJournal.Shared.Services;

namespace PatientJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<SimulationHub> _hub;

    public SessionsController(AppDbContext db, IHubContext<SimulationHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromQuery] int caseId)
    {
        var simCase = await _db.Cases.Include(c => c.Medications).FirstOrDefaultAsync(c => c.Id == caseId);
        if (simCase is null) return NotFound(new { message = "Case not found." });

        // End any currently active sessions before starting a new one
        var activeSessions = await _db.Sessions.Where(s => s.IsActive).ToListAsync();
        foreach (var s in activeSessions)
        {
            s.IsActive = false;
            s.EndedAt = DateTime.UtcNow;
        }

        var session = new SimulationSession
        {
            SimulationCaseId = caseId,
            StartedAt = DateTime.UtcNow,
            IsActive = true
        };
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        // Record initial vitals
        var initialVitals = new VitalReading
        {
            SessionId = session.Id,
            Timestamp = DateTime.UtcNow,
            SystolicBP = simCase.InitialSystolicBP,
            DiastolicBP = simCase.InitialDiastolicBP,
            HeartRate = simCase.InitialHeartRate,
            RespiratoryRate = simCase.InitialRespiratoryRate,
            OxygenSaturation = simCase.InitialOxygenSaturation,
            TemperatureCelsius = simCase.InitialTemperatureCelsius
        };
        _db.VitalReadings.Add(initialVitals);
        await _db.SaveChangesAsync();

        return Ok(new SessionSummaryDto(session.Id, caseId, simCase.Title, session.StartedAt, null, true));
    }

    [HttpPost("{id}/end")]
    public async Task<IActionResult> End(int id)
    {
        // End the specified session and any other orphaned active sessions
        var sessions = await _db.Sessions.Where(s => s.Id == id || s.IsActive).ToListAsync();
        foreach (var s in sessions)
        {
            s.IsActive = false;
            s.EndedAt ??= DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        await _hub.Clients.Group($"session-{id}").SendAsync("SessionEnded", id);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var session = await _db.Sessions
            .Include(s => s.SimulationCase)
            .Include(s => s.Interventions)
            .Include(s => s.VitalReadings)
            .Include(s => s.TeacherObservations)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (session is null) return NotFound();
        return Ok(session);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var session = await _db.Sessions
            .Include(s => s.SimulationCase).ThenInclude(c => c!.Medications)
            .Include(s => s.VitalReadings)
            .Include(s => s.Interventions)
            .Include(s => s.TeacherObservations)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();
        if (session is null) return NotFound(new { message = "No active session." });
        return Ok(session);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await _db.Sessions
            .Include(s => s.SimulationCase)
            .OrderByDescending(s => s.StartedAt)
            .Select(s => new SessionSummaryDto(
                s.Id, s.SimulationCaseId, s.SimulationCase!.Title,
                s.StartedAt, s.EndedAt, s.IsActive))
            .ToListAsync();
        return Ok(sessions);
    }

    [HttpPost("{id}/interventions")]
    public async Task<IActionResult> RegisterIntervention(int id, [FromBody] RegisterInterventionRequest request)
    {
        var session = await _db.Sessions
            .Include(s => s.SimulationCase).ThenInclude(c => c!.Medications)
            .Include(s => s.VitalReadings)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null || !session.IsActive) return NotFound(new { message = "Session not found or not active." });
        if (session.SimulationCase is null) return BadRequest(new { message = "Session has no associated case." });

        if (!Enum.TryParse<InterventionType>(request.Type, true, out var iType))
            return BadRequest(new { message = "Invalid intervention type." });

        var intervention = new NursingIntervention
        {
            SessionId = id,
            Timestamp = DateTime.UtcNow,
            Type = iType,
            DrugName = request.DrugName,
            Dose = request.Dose,
            Route = request.Route,
            FluidType = request.FluidType,
            FluidVolumeML = request.FluidVolumeML,
            Description = request.Description
        };

        // Validate
        var warning = PhysiologyEngine.ValidateIntervention(intervention, session.SimulationCase);
        intervention.ValidationWarning = warning;
        intervention.WasValid = warning is null;

        _db.Interventions.Add(intervention);

        // Apply physiological effect to last vitals
        var lastVitals = session.VitalReadings.OrderByDescending(v => v.Timestamp).FirstOrDefault();
        if (lastVitals is not null)
        {
            var newVitals = PhysiologyEngine.ApplyIntervention(lastVitals, intervention);
            newVitals.SessionId = id;
            _db.VitalReadings.Add(newVitals);
        }

        await _db.SaveChangesAsync();

        var dto = MapIntervention(intervention);
        await _hub.Clients.Group($"session-{id}").SendAsync("InterventionRegistered", dto);

        // Also broadcast updated vitals
        var updatedVitals = await _db.VitalReadings
            .Where(v => v.SessionId == id)
            .OrderByDescending(v => v.Timestamp)
            .FirstOrDefaultAsync();
        if (updatedVitals is not null)
            await _hub.Clients.Group($"session-{id}").SendAsync("VitalsUpdated", MapVitals(updatedVitals));

        return Ok(dto);
    }

    [HttpGet("{id}/interventions")]
    public async Task<IActionResult> GetInterventions(int id)
    {
        var items = await _db.Interventions
            .Where(i => i.SessionId == id)
            .OrderBy(i => i.Timestamp)
            .ToListAsync();
        return Ok(items.Select(MapIntervention));
    }

    [HttpGet("{id}/vitals")]
    public async Task<IActionResult> GetVitals(int id)
    {
        var items = await _db.VitalReadings
            .Where(v => v.SessionId == id)
            .OrderBy(v => v.Timestamp)
            .ToListAsync();
        return Ok(items.Select(MapVitals));
    }

    [HttpPost("{id}/observations")]
    public async Task<IActionResult> AddObservation(int id, [FromBody] AddObservationRequest request)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session is null) return NotFound();

        var obs = new TeacherObservation
        {
            SessionId = id,
            Note = request.Note,
            Timestamp = DateTime.UtcNow
        };
        _db.TeacherObservations.Add(obs);
        await _db.SaveChangesAsync();

        var dto = new TeacherObservationDto(obs.Id, obs.SessionId, obs.Timestamp, obs.Note);
        await _hub.Clients.Group($"session-{id}").SendAsync("ObservationAdded", dto);
        return Ok(dto);
    }

    [HttpGet("{id}/observations")]
    public async Task<IActionResult> GetObservations(int id)
    {
        var items = await _db.TeacherObservations
            .Where(o => o.SessionId == id)
            .OrderBy(o => o.Timestamp)
            .Select(o => new TeacherObservationDto(o.Id, o.SessionId, o.Timestamp, o.Note))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}/debrief")]
    public async Task<IActionResult> GetDebrief(int id)
    {
        var session = await _db.Sessions
            .Include(s => s.SimulationCase).ThenInclude(c => c!.Medications)
            .Include(s => s.Interventions)
            .Include(s => s.TeacherObservations)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null) return NotFound();

        var interventions = session.Interventions.OrderBy(i => i.Timestamp).Select(MapIntervention).ToList();
        var observations = session.TeacherObservations.OrderBy(o => o.Timestamp)
            .Select(o => new TeacherObservationDto(o.Id, o.SessionId, o.Timestamp, o.Note)).ToList();

        var deviations = new List<string>();
        if (session.SimulationCase is not null)
        {
            foreach (var intervention in session.Interventions.Where(i => !i.WasValid))
            {
                deviations.Add($"[{intervention.Timestamp:HH:mm:ss}] {intervention.ValidationWarning}");
            }

            // Check if medications from case were administered
            foreach (var med in session.SimulationCase.Medications)
            {
                var administered = session.Interventions.Any(i =>
                    i.Type == InterventionType.Medication &&
                    i.DrugName != null &&
                    i.DrugName.ToLower().Contains(med.DrugName.ToLower()));
                if (!administered)
                    deviations.Add($"Expected medication not administered: {med.DrugName} {med.Dosage} {med.Route}");
            }
        }

        var summary = new SessionSummaryDto(
            session.Id, session.SimulationCaseId,
            session.SimulationCase?.Title ?? "Unknown",
            session.StartedAt, session.EndedAt, session.IsActive);

        return Ok(new DebriefReport(summary, interventions, observations, deviations));
    }

    private static InterventionDto MapIntervention(NursingIntervention i) => new(
        i.Id, i.SessionId, i.Timestamp,
        i.Type.ToString(), i.DrugName, i.Dose, i.Route,
        i.FluidType, i.FluidVolumeML, i.Description,
        i.WasValid, i.ValidationWarning);

    private static VitalReadingDto MapVitals(VitalReading v) => new(
        v.Id, v.SessionId, v.Timestamp,
        v.SystolicBP, v.DiastolicBP, v.HeartRate,
        v.RespiratoryRate, v.OxygenSaturation, v.TemperatureCelsius);
}

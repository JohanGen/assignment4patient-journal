using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientJournal.API.Data;
using PatientJournal.Shared.DTOs;
using PatientJournal.Shared.Models;

namespace PatientJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CasesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CasesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] string? role)
    {
        var query = _db.Cases.AsQueryable();

        // Students can only see non-teacher-only cases (or cases they created)
        if (role == "Student" && userId.HasValue)
            query = query.Where(c => !c.IsTeacherOnly || c.CreatedByUserId == userId.Value);

        var list = await query.Select(c => new CaseListItem(
            c.Id, c.Title, c.PatientName, c.PatientAge, c.IsActive, c.IsTeacherOnly))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await _db.Cases
            .Include(x => x.Medications)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        return Ok(c);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var c = await _db.Cases
            .Include(x => x.Medications)
            .FirstOrDefaultAsync(x => x.IsActive);
        if (c is null) return NotFound(new { message = "No active case." });
        return Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SimulationCase simCase)
    {
        simCase.Id = 0;
        foreach (var med in simCase.Medications) med.Id = 0;
        simCase.CreatedAt = DateTime.UtcNow;
        _db.Cases.Add(simCase);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = simCase.Id }, simCase);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SimulationCase updated, [FromQuery] int userId, [FromQuery] string role)
    {
        var existing = await _db.Cases.Include(c => c.Medications).FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return NotFound();

        // Teacher-only cases can only be edited by teachers
        if (existing.IsTeacherOnly && role != "Teacher")
            return Forbid();

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.IsTeacherOnly = updated.IsTeacherOnly;
        existing.PatientName = updated.PatientName;
        existing.PatientAge = updated.PatientAge;
        existing.PatientSex = updated.PatientSex;
        existing.PatientWeightKg = updated.PatientWeightKg;
        existing.MedicalHistory = updated.MedicalHistory;
        existing.CurrentDiagnoses = updated.CurrentDiagnoses;
        existing.Allergies = updated.Allergies;
        existing.Contraindications = updated.Contraindications;
        existing.LabValues = updated.LabValues;
        existing.Goals = updated.Goals;
        existing.GoalTimerMinutes = updated.GoalTimerMinutes;
        existing.InitialSystolicBP = updated.InitialSystolicBP;
        existing.InitialDiastolicBP = updated.InitialDiastolicBP;
        existing.InitialHeartRate = updated.InitialHeartRate;
        existing.InitialRespiratoryRate = updated.InitialRespiratoryRate;
        existing.InitialOxygenSaturation = updated.InitialOxygenSaturation;
        existing.InitialTemperatureCelsius = updated.InitialTemperatureCelsius;

        // Replace medications
        _db.CaseMedications.RemoveRange(existing.Medications);
        existing.Medications = updated.Medications.Select(m => new CaseMedication
        {
            SimulationCaseId = id,
            DrugName = m.DrugName,
            Dosage = m.Dosage,
            Route = m.Route,
            Frequency = m.Frequency
        }).ToList();

        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(int id, [FromQuery] string role)
    {
        if (role != "Teacher") return Forbid();

        var target = await _db.Cases.FindAsync(id);
        if (target is null) return NotFound();
        target.IsActive = true;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, [FromQuery] string role)
    {
        if (role != "Teacher") return Forbid();
        var c = await _db.Cases.FindAsync(id);
        if (c is null) return NotFound();
        c.IsActive = false;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] string role)
    {
        if (role != "Teacher") return Forbid();
        var c = await _db.Cases.FindAsync(id);
        if (c is null) return NotFound();
        _db.Cases.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

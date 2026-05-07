using PatientJournal.Shared.Models;

namespace PatientJournal.Shared.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(int UserId, string Username, string Role, string Token);

public record CaseListItem(int Id, string Title, string PatientName, int PatientAge, bool IsActive, bool IsTeacherOnly);

public record VitalReadingDto(
    int Id, int SessionId, DateTime Timestamp,
    int SystolicBP, int DiastolicBP, int HeartRate,
    int RespiratoryRate, double OxygenSaturation, double TemperatureCelsius);

public record InterventionDto(
    int Id, int SessionId, DateTime Timestamp,
    string Type, string? DrugName, string? Dose, string? Route,
    string? FluidType, int? FluidVolumeML, string? Description,
    bool WasValid, string? ValidationWarning);

public record RegisterInterventionRequest(
    int SessionId,
    string Type,
    string? DrugName, string? Dose, string? Route,
    string? FluidType, int? FluidVolumeML,
    string? Description);

public record TeacherObservationDto(int Id, int SessionId, DateTime Timestamp, string Note);
public record AddObservationRequest(int SessionId, string Note);
public record SessionSummaryDto(int SessionId, int CaseId, string CaseTitle, DateTime StartedAt, DateTime? EndedAt, bool IsActive);
public record DebriefReport(
    SessionSummaryDto Session,
    List<InterventionDto> Interventions,
    List<TeacherObservationDto> Observations,
    List<string> Deviations);

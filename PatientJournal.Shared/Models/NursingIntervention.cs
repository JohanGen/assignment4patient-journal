namespace PatientJournal.Shared.Models;

public class NursingIntervention
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public InterventionType Type { get; set; }

    // Medication fields
    public string? DrugName { get; set; }
    public string? Dose { get; set; }
    public string? Route { get; set; }

    // Fluid fields
    public string? FluidType { get; set; }
    public int? FluidVolumeML { get; set; }

    // Other intervention
    public string? Description { get; set; }

    // Validation result
    public bool WasValid { get; set; } = true;
    public string? ValidationWarning { get; set; }

    public SimulationSession? Session { get; set; }
}

public enum InterventionType
{
    Medication,
    Fluid,
    Other
}

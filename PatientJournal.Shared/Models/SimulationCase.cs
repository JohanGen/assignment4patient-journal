namespace PatientJournal.Shared.Models;

public class SimulationCase
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsTeacherOnly { get; set; }
    public bool IsActive { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Patient Demographics
    public string PatientName { get; set; } = string.Empty;
    public int PatientAge { get; set; }
    public string PatientSex { get; set; } = string.Empty;
    public double PatientWeightKg { get; set; }

    // Medical Info
    public string MedicalHistory { get; set; } = string.Empty;
    public string CurrentDiagnoses { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string Contraindications { get; set; } = string.Empty;
    public string LabValues { get; set; } = string.Empty;
    public string Goals { get; set; } = string.Empty;
    public int GoalTimerMinutes { get; set; } = 10;

    // Starting Vitals
    public int InitialSystolicBP { get; set; }
    public int InitialDiastolicBP { get; set; }
    public int InitialHeartRate { get; set; }
    public int InitialRespiratoryRate { get; set; }
    public double InitialOxygenSaturation { get; set; }
    public double InitialTemperatureCelsius { get; set; }

    public List<CaseMedication> Medications { get; set; } = new();
    public List<SimulationSession> Sessions { get; set; } = new();
}

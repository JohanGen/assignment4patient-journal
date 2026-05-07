namespace PatientJournal.Shared.Models;

public class SimulationSession
{
    public int Id { get; set; }
    public int SimulationCaseId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public SimulationCase? SimulationCase { get; set; }
    public List<NursingIntervention> Interventions { get; set; } = new();
    public List<VitalReading> VitalReadings { get; set; } = new();
    public List<TeacherObservation> TeacherObservations { get; set; } = new();
}

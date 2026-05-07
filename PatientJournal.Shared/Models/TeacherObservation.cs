namespace PatientJournal.Shared.Models;

public class TeacherObservation
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Note { get; set; } = string.Empty;
    public SimulationSession? Session { get; set; }
}

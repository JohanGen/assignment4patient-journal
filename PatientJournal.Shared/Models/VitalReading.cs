namespace PatientJournal.Shared.Models;

public class VitalReading
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int SystolicBP { get; set; }
    public int DiastolicBP { get; set; }
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public double OxygenSaturation { get; set; }
    public double TemperatureCelsius { get; set; }
    public SimulationSession? Session { get; set; }
}

namespace PatientJournal.Shared.Models;

public class CaseMedication
{
    public int Id { get; set; }
    public int SimulationCaseId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public SimulationCase? SimulationCase { get; set; }
}

using PatientJournal.Shared.Models;

namespace PatientJournal.Shared.Services;

/// <summary>
/// Rule-based physiological simulation engine.
/// Applies simplified vital sign changes based on nursing interventions.
/// </summary>
public static class PhysiologyEngine
{
    public static VitalReading ApplyIntervention(VitalReading current, NursingIntervention intervention)
    {
        var next = Clone(current);
        next.Timestamp = DateTime.UtcNow;

        if (intervention.Type == InterventionType.Medication && intervention.DrugName is not null)
        {
            var drug = intervention.DrugName.ToLower().Trim();
            ApplyMedicationEffect(next, drug);
        }
        else if (intervention.Type == InterventionType.Fluid)
        {
            // IV fluids increase BP slightly and normalize HR
            next.SystolicBP = Clamp(next.SystolicBP + 8, 60, 220);
            next.DiastolicBP = Clamp(next.DiastolicBP + 4, 40, 140);
            next.HeartRate = Clamp(next.HeartRate - 5, 40, 180);
        }
        else if (intervention.Type == InterventionType.Other && intervention.Description is not null)
        {
            var desc = intervention.Description.ToLower().Trim();
            if (desc.Contains("oxygen") || desc.Contains("o2"))
            {
                next.OxygenSaturation = Math.Min(100, next.OxygenSaturation + 5);
                next.RespiratoryRate = Clamp(next.RespiratoryRate - 2, 8, 40);
            }
            if (desc.Contains("reposition"))
            {
                next.OxygenSaturation = Math.Min(100, next.OxygenSaturation + 2);
            }
        }

        return next;
    }

    private static void ApplyMedicationEffect(VitalReading vitals, string drug)
    {
        // Antihypertensives / vasodilators
        if (drug.Contains("metoprolol") || drug.Contains("atenolol") || drug.Contains("bisoprolol"))
        {
            vitals.HeartRate = Clamp(vitals.HeartRate - 15, 40, 180);
            vitals.SystolicBP = Clamp(vitals.SystolicBP - 10, 60, 220);
        }
        else if (drug.Contains("amlodipine") || drug.Contains("nifedipine"))
        {
            vitals.SystolicBP = Clamp(vitals.SystolicBP - 15, 60, 220);
            vitals.DiastolicBP = Clamp(vitals.DiastolicBP - 8, 40, 140);
        }
        else if (drug.Contains("nitroglycerin") || drug.Contains("nitro"))
        {
            vitals.SystolicBP = Clamp(vitals.SystolicBP - 20, 60, 220);
            vitals.DiastolicBP = Clamp(vitals.DiastolicBP - 10, 40, 140);
            vitals.HeartRate = Clamp(vitals.HeartRate + 10, 40, 180);
        }
        // Vasopressors
        else if (drug.Contains("norepinephrine") || drug.Contains("noradrenaline"))
        {
            vitals.SystolicBP = Clamp(vitals.SystolicBP + 25, 60, 220);
            vitals.DiastolicBP = Clamp(vitals.DiastolicBP + 15, 40, 140);
            vitals.HeartRate = Clamp(vitals.HeartRate - 5, 40, 180);
        }
        else if (drug.Contains("adrenaline") || drug.Contains("epinephrine"))
        {
            vitals.SystolicBP = Clamp(vitals.SystolicBP + 30, 60, 220);
            vitals.HeartRate = Clamp(vitals.HeartRate + 20, 40, 180);
        }
        // Antipyretics
        else if (drug.Contains("paracetamol") || drug.Contains("acetaminophen") || drug.Contains("ibuprofen"))
        {
            vitals.TemperatureCelsius = Math.Max(36.5, vitals.TemperatureCelsius - 1.0);
            vitals.HeartRate = Clamp(vitals.HeartRate - 5, 40, 180);
        }
        // Opioids / sedatives
        else if (drug.Contains("morphine") || drug.Contains("fentanyl") || drug.Contains("diazepam"))
        {
            vitals.RespiratoryRate = Clamp(vitals.RespiratoryRate - 4, 6, 40);
            vitals.OxygenSaturation = Math.Max(70, vitals.OxygenSaturation - 3);
            vitals.HeartRate = Clamp(vitals.HeartRate - 8, 40, 180);
            vitals.SystolicBP = Clamp(vitals.SystolicBP - 10, 60, 220);
        }
        // Atropine
        else if (drug.Contains("atropine"))
        {
            vitals.HeartRate = Clamp(vitals.HeartRate + 20, 40, 180);
        }
        // Bronchodilators
        else if (drug.Contains("salbutamol") || drug.Contains("albuterol") || drug.Contains("ventolin"))
        {
            vitals.RespiratoryRate = Clamp(vitals.RespiratoryRate - 4, 8, 40);
            vitals.OxygenSaturation = Math.Min(100, vitals.OxygenSaturation + 4);
            vitals.HeartRate = Clamp(vitals.HeartRate + 8, 40, 180);
        }
        // Insulin
        else if (drug.Contains("insulin"))
        {
            // No direct vital effect modeled
        }
        // Default: minor effect
        else
        {
            vitals.HeartRate = Clamp(vitals.HeartRate - 2, 40, 180);
        }
    }

    public static string? ValidateIntervention(NursingIntervention intervention, SimulationCase simCase)
    {
        if (intervention.Type != InterventionType.Medication || intervention.DrugName is null)
            return null;

        var drug = intervention.DrugName.ToLower().Trim();
        var allergies = simCase.Allergies.ToLower();
        var contraindications = simCase.Contraindications.ToLower();

        foreach (var allergen in allergies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (drug.Contains(allergen) || allergen.Contains(drug))
                return $"ALLERGY WARNING: Patient is allergic to {allergen}.";
        }

        foreach (var ci in contraindications.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (drug.Contains(ci) || ci.Contains(drug))
                return $"CONTRAINDICATION: {drug} is contraindicated for this patient ({ci}).";
        }

        return null;
    }

    private static VitalReading Clone(VitalReading v) => new()
    {
        SessionId = v.SessionId,
        SystolicBP = v.SystolicBP,
        DiastolicBP = v.DiastolicBP,
        HeartRate = v.HeartRate,
        RespiratoryRate = v.RespiratoryRate,
        OxygenSaturation = v.OxygenSaturation,
        TemperatureCelsius = v.TemperatureCelsius
    };

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}

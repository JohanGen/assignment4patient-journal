using Microsoft.EntityFrameworkCore;
using PatientJournal.Shared.Models;

namespace PatientJournal.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<SimulationCase> Cases => Set<SimulationCase>();
    public DbSet<CaseMedication> CaseMedications => Set<CaseMedication>();
    public DbSet<SimulationSession> Sessions => Set<SimulationSession>();
    public DbSet<NursingIntervention> Interventions => Set<NursingIntervention>();
    public DbSet<VitalReading> VitalReadings => Set<VitalReading>();
    public DbSet<TeacherObservation> TeacherObservations => Set<TeacherObservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SimulationCase>()
            .HasMany(c => c.Medications)
            .WithOne(m => m.SimulationCase)
            .HasForeignKey(m => m.SimulationCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SimulationCase>()
            .HasMany(c => c.Sessions)
            .WithOne(s => s.SimulationCase)
            .HasForeignKey(s => s.SimulationCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SimulationSession>()
            .HasMany(s => s.Interventions)
            .WithOne(i => i.Session)
            .HasForeignKey(i => i.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SimulationSession>()
            .HasMany(s => s.VitalReadings)
            .WithOne(v => v.Session)
            .HasForeignKey(v => v.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SimulationSession>()
            .HasMany(s => s.TeacherObservations)
            .WithOne(o => o.Session)
            .HasForeignKey(o => o.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed default users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "teacher", PasswordHash = BCrypt("teacher123"), Role = UserRole.Teacher },
            new User { Id = 2, Username = "student", PasswordHash = BCrypt("student123"), Role = UserRole.Student }
        );

        // Seed a pre-made teacher-only case
        modelBuilder.Entity<SimulationCase>().HasData(new SimulationCase
        {
            Id = 1,
            Title = "Hypertensive Emergency",
            Description = "Patient presents with severe hypertension and headache.",
            IsTeacherOnly = true,
            IsActive = false,
            CreatedByUserId = 1,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PatientName = "Anna Hansen",
            PatientAge = 58,
            PatientSex = "Female",
            PatientWeightKg = 72,
            MedicalHistory = "Chronic hypertension, type 2 diabetes",
            CurrentDiagnoses = "Hypertensive emergency, headache",
            Allergies = "penicillin",
            Contraindications = "beta-blocker",
            LabValues = "Creatinine: 110 µmol/L, K+: 4.1 mmol/L",
            Goals = "Reduce systolic BP below 160 mmHg within 10 minutes",
            GoalTimerMinutes = 10,
            InitialSystolicBP = 210,
            InitialDiastolicBP = 120,
            InitialHeartRate = 95,
            InitialRespiratoryRate = 18,
            InitialOxygenSaturation = 96,
            InitialTemperatureCelsius = 37.1
        });

        modelBuilder.Entity<SimulationCase>().HasData(new SimulationCase
        {
            Id = 2,
            Title = "Septic Shock",
            Description = "Patient with fever, low blood pressure and elevated heart rate.",
            IsTeacherOnly = true,
            IsActive = false,
            CreatedByUserId = 1,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PatientName = "Lars Eriksen",
            PatientAge = 72,
            PatientSex = "Male",
            PatientWeightKg = 85,
            MedicalHistory = "COPD, prostate cancer",
            CurrentDiagnoses = "Septic shock (suspected urinary source)",
            Allergies = "sulfonamides",
            Contraindications = "",
            LabValues = "WBC: 18.2, Lactate: 4.1 mmol/L, CRP: 320",
            Goals = "Achieve MAP >65 mmHg, administer antibiotics and 30 ml/kg IV fluid within 15 minutes",
            GoalTimerMinutes = 15,
            InitialSystolicBP = 82,
            InitialDiastolicBP = 50,
            InitialHeartRate = 128,
            InitialRespiratoryRate = 26,
            InitialOxygenSaturation = 91,
            InitialTemperatureCelsius = 39.4
        });

        modelBuilder.Entity<CaseMedication>().HasData(
            new CaseMedication { Id = 1, SimulationCaseId = 1, DrugName = "Amlodipine", Dosage = "5mg", Route = "PO", Frequency = "Daily" },
            new CaseMedication { Id = 2, SimulationCaseId = 1, DrugName = "Metformin", Dosage = "500mg", Route = "PO", Frequency = "Twice daily" },
            new CaseMedication { Id = 3, SimulationCaseId = 2, DrugName = "Norepinephrine", Dosage = "0.1 mcg/kg/min", Route = "IV", Frequency = "Continuous" },
            new CaseMedication { Id = 4, SimulationCaseId = 2, DrugName = "Piperacillin-Tazobactam", Dosage = "4.5g", Route = "IV", Frequency = "Every 6 hours" }
        );
    }

    private static string BCrypt(string password)
    {
        // Simple deterministic hash for seeding - in prod use proper BCrypt
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "pj_salt"));
        return Convert.ToHexString(bytes);
    }
}

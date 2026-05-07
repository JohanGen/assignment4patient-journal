using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PatientJournal.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsTeacherOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientAge = table.Column<int>(type: "int", nullable: false),
                    PatientSex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientWeightKg = table.Column<double>(type: "float", nullable: false),
                    MedicalHistory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentDiagnoses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contraindications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Goals = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalTimerMinutes = table.Column<int>(type: "int", nullable: false),
                    InitialSystolicBP = table.Column<int>(type: "int", nullable: false),
                    InitialDiastolicBP = table.Column<int>(type: "int", nullable: false),
                    InitialHeartRate = table.Column<int>(type: "int", nullable: false),
                    InitialRespiratoryRate = table.Column<int>(type: "int", nullable: false),
                    InitialOxygenSaturation = table.Column<double>(type: "float", nullable: false),
                    InitialTemperatureCelsius = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseMedications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimulationCaseId = table.Column<int>(type: "int", nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseMedications_Cases_SimulationCaseId",
                        column: x => x.SimulationCaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimulationCaseId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Cases_SimulationCaseId",
                        column: x => x.SimulationCaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interventions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FluidType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FluidVolumeML = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasValid = table.Column<bool>(type: "bit", nullable: false),
                    ValidationWarning = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interventions_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherObservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherObservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherObservations_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VitalReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SystolicBP = table.Column<int>(type: "int", nullable: false),
                    DiastolicBP = table.Column<int>(type: "int", nullable: false),
                    HeartRate = table.Column<int>(type: "int", nullable: false),
                    RespiratoryRate = table.Column<int>(type: "int", nullable: false),
                    OxygenSaturation = table.Column<double>(type: "float", nullable: false),
                    TemperatureCelsius = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VitalReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VitalReadings_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cases",
                columns: new[] { "Id", "Allergies", "Contraindications", "CreatedAt", "CreatedByUserId", "CurrentDiagnoses", "Description", "GoalTimerMinutes", "Goals", "InitialDiastolicBP", "InitialHeartRate", "InitialOxygenSaturation", "InitialRespiratoryRate", "InitialSystolicBP", "InitialTemperatureCelsius", "IsActive", "IsTeacherOnly", "LabValues", "MedicalHistory", "PatientAge", "PatientName", "PatientSex", "PatientWeightKg", "Title" },
                values: new object[,]
                {
                    { 1, "penicillin", "beta-blocker", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Hypertensive emergency, headache", "Patient presents with severe hypertension and headache.", 10, "Reduce systolic BP below 160 mmHg within 10 minutes", 120, 95, 96.0, 18, 210, 37.100000000000001, false, true, "Creatinine: 110 µmol/L, K+: 4.1 mmol/L", "Chronic hypertension, type 2 diabetes", 58, "Anna Hansen", "Female", 72.0, "Hypertensive Emergency" },
                    { 2, "sulfonamides", "", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Septic shock (suspected urinary source)", "Patient with fever, low blood pressure and elevated heart rate.", 15, "Achieve MAP >65 mmHg, administer antibiotics and 30 ml/kg IV fluid within 15 minutes", 50, 128, 91.0, 26, 82, 39.399999999999999, false, true, "WBC: 18.2, Lactate: 4.1 mmol/L, CRP: 320", "COPD, prostate cancer", 72, "Lars Eriksen", "Male", 85.0, "Septic Shock" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "341A5FB36B5039AE25BB5D0A5B90F78A13259825A23C33B58C0A71F1EA13A120", 1, "teacher" },
                    { 2, "0D08D34EBA80E6B3644EB1620C9F8499D34D5A49E45A6681CF99D5D297ED6A36", 0, "student" }
                });

            migrationBuilder.InsertData(
                table: "CaseMedications",
                columns: new[] { "Id", "Dosage", "DrugName", "Frequency", "Route", "SimulationCaseId" },
                values: new object[,]
                {
                    { 1, "5mg", "Amlodipine", "Daily", "PO", 1 },
                    { 2, "500mg", "Metformin", "Twice daily", "PO", 1 },
                    { 3, "0.1 mcg/kg/min", "Norepinephrine", "Continuous", "IV", 2 },
                    { 4, "4.5g", "Piperacillin-Tazobactam", "Every 6 hours", "IV", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseMedications_SimulationCaseId",
                table: "CaseMedications",
                column: "SimulationCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_SessionId",
                table: "Interventions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SimulationCaseId",
                table: "Sessions",
                column: "SimulationCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherObservations_SessionId",
                table: "TeacherObservations",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_VitalReadings_SessionId",
                table: "VitalReadings",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseMedications");

            migrationBuilder.DropTable(
                name: "Interventions");

            migrationBuilder.DropTable(
                name: "TeacherObservations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VitalReadings");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Cases");
        }
    }
}

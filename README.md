# Patient Journal System – HVL Nursing Simulation Training

## Architecture Overview

### Technology Choices and Rationale

| Component | Technology | Port | Notes |
|---|---|---|---|
| **API** | ASP.NET Core Web API | 5200 | Central backend; all apps communicate through this |
| **App 1 – Case Setup** | ASP.NET Core MVC | 5152 | Teacher/student web interface for case management |
| **App 2 – Simulation** | WPF Desktop | N/A | Student simulation interface with real-time vitals |
| **App 3 – Assessment** | Blazor Server | 5165 | Teacher live monitoring and debrief tool |
| **Shared Library** | .NET Class Library | N/A | Models, DTOs, PhysiologyEngine |
| **Database** | SQL Server LocalDB | N/A | Persists all data via EF Core migrations |
| **Real-time** | SignalR | /hubs/simulation | Live intervention/observation updates |

### Key Architectural Decisions

1. **Centralised API approach**: All three apps communicate exclusively through the REST API + SignalR hub. No app accesses the database directly. This ensures a single source of truth and allows heterogeneous clients (MVC, WPF, Blazor) to co-exist.

2. **Shared library**: `PatientJournal.Shared` contains all domain models, DTOs, and the `PhysiologyEngine`. This avoids code duplication across projects.

3. **SignalR for real-time**: When a student registers an intervention via the WPF app, the API broadcasts the update to all connected clients (including the Blazor teacher view) via SignalR groups keyed by session ID.

4. **Simple authentication**: Cookie-based auth in MVC (App 1). The token is a base64-encoded `userId:role` string passed as query params to the API – sufficient for a simulation training context where security is not a priority. Teachers can activate cases and manage sessions; students can create/edit non-protected cases and view assigned cases.

5. **Rule-based physiology**: `PhysiologyEngine.ApplyIntervention()` applies deterministic vital sign changes based on drug class (antihypertensives, vasopressors, antipyretics, opioids, etc.). This is intentionally simplified but structured so that new drug rules can be added without architectural changes.

---

## Prerequisites

- .NET 10 SDK
- SQL Server LocalDB (installed with Visual Studio)
- Visual Studio 2022/2026 or VS Code

---

## Getting Started

### 1. Start the API (required first)

```bash
cd PatientJournal.API
dotnet run --launch-profile http
# Runs on http://localhost:5200
```

The API automatically applies database migrations and seeds:
- **teacher / teacher123** (Teacher role)
- **student / student123** (Student role)
- Two pre-made teacher-only cases: "Hypertensive Emergency" and "Septic Shock"

### 2. Start the Web App (App 1 – Case Setup)

```bash
cd PatientJournal.Web
dotnet run --launch-profile http
# Open http://localhost:5152
```

Log in as `teacher` to create/activate cases. Log in as `student` to view available cases.

### 3. Start the Assessment App (App 3 – Teacher View)

```bash
cd PatientJournal.Assessment
dotnet run --launch-profile http
# Open http://localhost:5165
```

The teacher can monitor the live session, add timestamped observations, and view the debrief report.

### 4. Start the Desktop App (App 2 – Simulation Interface)

```bash
cd PatientJournal.Desktop
dotnet run
```

Click **"Load Active Session"** to fetch the currently active case/session. Then register nursing interventions and observe real-time vital sign changes.

---

## Workflow

1. **Teacher (Web App)**: Create or select a case → Activate it → Start a session
2. **Student (Desktop App)**: Load active session → View patient info → Register interventions
3. **Teacher (Assessment App)**: Monitor live student actions → Add observations → Export debrief after session ends

---

## Project Structure

```
PatientJournal/
├── PatientJournal.Shared/          # Class library
│   ├── Models/                     # Domain entities (EF Core)
│   ├── DTOs/                       # Request/response objects
│   └── Services/PhysiologyEngine   # Rule-based vital simulation
├── PatientJournal.API/             # ASP.NET Core Web API
│   ├── Controllers/                # Auth, Cases, Sessions
│   ├── Data/AppDbContext           # EF Core DbContext + seeding
│   └── Hubs/SimulationHub          # SignalR hub
├── PatientJournal.Web/             # ASP.NET Core MVC (App 1)
│   ├── Controllers/                # Account, Cases
│   ├── Services/ApiService         # HTTP client wrapper
│   └── Views/                      # Razor views
├── PatientJournal.Desktop/         # WPF (App 2)
│   ├── Services/ApiService         # HTTP client wrapper
│   └── MainWindow.xaml/cs          # Full simulation UI
└── PatientJournal.Assessment/      # Blazor Server (App 3)
    ├── Services/                   # API + SignalR services
    └── Components/Pages/           # Home (live), Sessions, Debrief
```

---

## Potentially Unimplemented Features (Architecture Supports Extension)

- **Vital sign chart**: The `VitalReading` history is stored and returned by the API. A WPF `Canvas`/`OxyPlot` chart can be added to the Desktop app without API changes.
- **PDF export**: The Debrief page uses `window.print()`. A proper PDF export could use a reporting library (e.g., QuestPDF) in a new API endpoint `/sessions/{id}/debrief/pdf`.
- **More physiological rules**: Add new drug mappings in `PhysiologyEngine.ApplyMedicationEffect()` – no structural changes needed.
- **Student session assignment**: The schema supports multiple sessions per case. A teacher could assign a specific session to a specific student group via a new API endpoint.

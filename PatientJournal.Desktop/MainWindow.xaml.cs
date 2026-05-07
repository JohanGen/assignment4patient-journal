using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using PatientJournal.Desktop.Models;
using PatientJournal.Desktop.Services;
using PatientJournal.Shared.DTOs;
using PatientJournal.Shared.Models;

namespace PatientJournal.Desktop;

public partial class MainWindow : Window
{
    private readonly ApiService _api;
    private SimulationSession? _session;
    private SimulationCase? _case;
    private HubConnection? _hub;
    private DispatcherTimer? _goalTimer;
    private DateTime _sessionStart;
    private int _goalMinutes;
    private ObservableCollection<EventLogItem> _eventLog = new();

    public MainWindow()
    {
        InitializeComponent();
        _api = new ApiService(App.ApiBaseUrl);
        EventLogList.ItemsSource = _eventLog;
    }

    private async void LoadSessionBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadSessionBtn.IsEnabled = false;
        LoadSessionBtn.Content = "Loading...";

        _session = await _api.GetActiveSessionAsync();

        if (_session is null)
        {
            MessageBox.Show("No active session found.\n\nA teacher must activate a case and start a session from the web application.",
                "No Active Session", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadSessionBtn.IsEnabled = true;
            LoadSessionBtn.Content = "Load Active Session";
            return;
        }

        _case = _session.SimulationCase;
        await LoadSessionData();
        await ConnectToSignalR();
        LoadSessionBtn.Content = "Refresh";
        LoadSessionBtn.IsEnabled = true;
        EndSessionBtn.IsEnabled = true;
        RegisterBtn.IsEnabled = true;
    }

    private async Task LoadSessionData()
    {
        if (_session is null || _case is null) return;

        CaseTitleText.Text = $"📋 {_case.Title}";
        PatientInfoText.Text = $"{_case.PatientName} | {_case.PatientAge} yrs | {_case.PatientSex} | {_case.PatientWeightKg} kg";
        PatientDetailText.Text = $"Diagnoses: {_case.CurrentDiagnoses}\n\nHistory: {_case.MedicalHistory}\n\nLabs: {_case.LabValues}";
        AllergyText.Text = $"Allergies: {(string.IsNullOrWhiteSpace(_case.Allergies) ? "None" : _case.Allergies)}\nContraindications: {(string.IsNullOrWhiteSpace(_case.Contraindications) ? "None" : _case.Contraindications)}";
        CaseGoalText.Text = $"🎯 Goals: {_case.Goals}  |  Timer: {_case.GoalTimerMinutes} minutes";

        // Load vitals
        var vitals = await _api.GetVitalsAsync(_session.Id);
        if (vitals.Any())
            UpdateVitalsDisplay(vitals.Last());

        // Load event log
        _eventLog.Clear();
        var interventions = await _api.GetInterventionsAsync(_session.Id);
        foreach (var i in interventions)
            AddInterventionToLog(i);

        // Start goal timer
        _sessionStart = _session.StartedAt;
        _goalMinutes = _case.GoalTimerMinutes;
        StartGoalTimer();
    }

    private void StartGoalTimer()
    {
        _goalTimer?.Stop();
        _goalTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _goalTimer.Tick += (_, _) =>
        {
            var elapsed = DateTime.UtcNow - _sessionStart;
            var remaining = TimeSpan.FromMinutes(_goalMinutes) - elapsed;
            if (remaining.TotalSeconds > 0)
                GoalTimerText.Text = $"⏱ {remaining:mm\\:ss} remaining";
            else
            {
                GoalTimerText.Text = "⏱ TIME EXPIRED";
                GoalTimerText.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }
        };
        _goalTimer.Start();
    }

    private async Task ConnectToSignalR()
    {
        if (_session is null) return;
        _hub?.DisposeAsync();

        _hub = new HubConnectionBuilder()
            .WithUrl($"{App.ApiBaseUrl}/hubs/simulation")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<VitalReadingDto>("VitalsUpdated", dto =>
            Dispatcher.Invoke(() => UpdateVitalsDisplay(dto)));

        _hub.On<InterventionDto>("InterventionRegistered", dto =>
            Dispatcher.Invoke(() => AddInterventionToLog(dto)));

        _hub.On<object>("SessionEnded", _ =>
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("The session has been ended by the teacher.", "Session Ended");
                EndSessionBtn.IsEnabled = false;
                RegisterBtn.IsEnabled = false;
                _goalTimer?.Stop();
            }));

        try
        {
            await _hub.StartAsync();
            await _hub.InvokeAsync("JoinSession", _session.Id.ToString());
        }
        catch { /* SignalR connection failure is non-fatal */ }
    }

    private void UpdateVitalsDisplay(VitalReadingDto dto)
    {
        BPText.Text = $"{dto.SystolicBP}/{dto.DiastolicBP}";
        HRText.Text = dto.HeartRate.ToString();
        RRText.Text = dto.RespiratoryRate.ToString();
        SpO2Text.Text = $"{dto.OxygenSaturation:F1}";
        TempText.Text = $"{dto.TemperatureCelsius:F1}";
        LastUpdateText.Text = dto.Timestamp.ToLocalTime().ToString("HH:mm:ss");

        // Color coding
        BPText.Foreground = dto.SystolicBP > 180 || dto.SystolicBP < 90
            ? System.Windows.Media.Brushes.Red
            : System.Windows.Media.Brushes.DarkGreen;
        HRText.Foreground = dto.HeartRate > 100 || dto.HeartRate < 50
            ? System.Windows.Media.Brushes.OrangeRed
            : System.Windows.Media.Brushes.DarkSlateGray;
        SpO2Text.Foreground = dto.OxygenSaturation < 94
            ? System.Windows.Media.Brushes.Red
            : System.Windows.Media.Brushes.DarkGreen;
    }

    private void UpdateVitalsDisplay(VitalReading vr) =>
        UpdateVitalsDisplay(new VitalReadingDto(vr.Id, vr.SessionId, vr.Timestamp,
            vr.SystolicBP, vr.DiastolicBP, vr.HeartRate,
            vr.RespiratoryRate, vr.OxygenSaturation, vr.TemperatureCelsius));

    private void AddInterventionToLog(InterventionDto dto)
    {
        string summary = dto.Type switch
        {
            "Medication" => $"💊 {dto.DrugName} {dto.Dose} {dto.Route}",
            "Fluid" => $"💧 {dto.FluidType} {dto.FluidVolumeML}mL",
            _ => $"🔧 {dto.Description}"
        };
        _eventLog.Insert(0, new EventLogItem
        {
            TimeStr = dto.Timestamp.ToLocalTime().ToString("HH:mm:ss"),
            Summary = summary,
            Warning = dto.WasValid ? null : dto.ValidationWarning
        });
    }

    private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_session is null) return;

        ValidationWarningBorder.Visibility = Visibility.Collapsed;
        RegisterBtn.IsEnabled = false;

        var type = ((ComboBoxItem)InterventionTypeCombo.SelectedItem).Content.ToString()!;

        var request = new RegisterInterventionRequest(
            _session.Id, type,
            type == "Medication" ? DrugNameBox.Text.Trim() : null,
            type == "Medication" ? DoseBox.Text.Trim() : null,
            type == "Medication" ? ((ComboBoxItem)RouteCombo.SelectedItem).Content.ToString() : null,
            type == "Fluid" ? ((ComboBoxItem)FluidTypeCombo.SelectedItem).Content.ToString() : null,
            type == "Fluid" && int.TryParse(FluidVolumeBox.Text, out var vol) ? vol : null,
            type == "Other" ? DescriptionBox.Text.Trim() : null
        );

        var result = await _api.RegisterInterventionAsync(request);

        if (result is null)
        {
            MessageBox.Show("Failed to register intervention. Is the API running?", "Error");
        }
        else if (!result.WasValid && result.ValidationWarning is not null)
        {
            ValidationWarningText.Text = "⚠️ " + result.ValidationWarning;
            ValidationWarningBorder.Visibility = Visibility.Visible;
        }

        // Clear form
        DrugNameBox.Text = "";
        DoseBox.Text = "";
        FluidVolumeBox.Text = "";
        DescriptionBox.Text = "";

        RegisterBtn.IsEnabled = true;
    }

    private void InterventionTypeCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (MedicationPanel is null) return;
        var type = ((ComboBoxItem)InterventionTypeCombo.SelectedItem).Content.ToString();
        MedicationPanel.Visibility = type == "Medication" ? Visibility.Visible : Visibility.Collapsed;
        FluidPanel.Visibility = type == "Fluid" ? Visibility.Visible : Visibility.Collapsed;
        OtherPanel.Visibility = type == "Other" ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void EndSessionBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_session is null) return;
        if (MessageBox.Show("End the simulation session?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        await _api.EndSessionAsync(_session.Id);
        EndSessionBtn.IsEnabled = false;
        RegisterBtn.IsEnabled = false;
        _goalTimer?.Stop();
        GoalTimerText.Text = "Session Ended";
    }

    protected override async void OnClosed(EventArgs e)
    {
        _goalTimer?.Stop();
        if (_hub is not null)
            await _hub.DisposeAsync();
        base.OnClosed(e);
    }
}
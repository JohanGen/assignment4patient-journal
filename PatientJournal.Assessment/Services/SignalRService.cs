using Microsoft.AspNetCore.SignalR.Client;
using PatientJournal.Shared.DTOs;

namespace PatientJournal.Assessment.Services;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hub;
    private readonly string _apiBase;

    public event Action<InterventionDto>? OnInterventionRegistered;
    public event Action<TeacherObservationDto>? OnObservationAdded;
    public event Action<int>? OnSessionEnded;

    public SignalRService(string apiBase)
    {
        _apiBase = apiBase;
    }

    public async Task JoinSessionAsync(int sessionId)
    {
        if (_hub is not null)
            await _hub.DisposeAsync();

        _hub = new HubConnectionBuilder()
            .WithUrl($"{_apiBase}/hubs/simulation")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<InterventionDto>("InterventionRegistered", dto => OnInterventionRegistered?.Invoke(dto));
        _hub.On<TeacherObservationDto>("ObservationAdded", dto => OnObservationAdded?.Invoke(dto));
        _hub.On<int>("SessionEnded", id => OnSessionEnded?.Invoke(id));

        await _hub.StartAsync();
        await _hub.InvokeAsync("JoinSession", sessionId.ToString());
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
            await _hub.DisposeAsync();
    }
}

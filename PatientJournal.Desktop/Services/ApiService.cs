using System.Net.Http;
using System.Net.Http.Json;
using PatientJournal.Shared.DTOs;
using PatientJournal.Shared.Models;

namespace PatientJournal.Desktop.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<SimulationSession?> GetActiveSessionAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<SimulationSession>("api/sessions/active");
        }
        catch { return null; }
    }

    public async Task<SessionSummaryDto?> StartSessionAsync(int caseId)
    {
        var response = await _http.PostAsync($"api/sessions/start?caseId={caseId}", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SessionSummaryDto>();
    }

    public async Task<bool> EndSessionAsync(int sessionId)
    {
        var response = await _http.PostAsync($"api/sessions/{sessionId}/end", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<InterventionDto?> RegisterInterventionAsync(RegisterInterventionRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/sessions/{request.SessionId}/interventions", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InterventionDto>();
    }

    public async Task<List<VitalReadingDto>> GetVitalsAsync(int sessionId)
    {
        var result = await _http.GetFromJsonAsync<List<VitalReadingDto>>($"api/sessions/{sessionId}/vitals");
        return result ?? new();
    }

    public async Task<List<InterventionDto>> GetInterventionsAsync(int sessionId)
    {
        var result = await _http.GetFromJsonAsync<List<InterventionDto>>($"api/sessions/{sessionId}/interventions");
        return result ?? new();
    }

    public string BaseUrl => _http.BaseAddress?.ToString() ?? "";
}

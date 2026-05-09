using System.Net.Http.Json;
using PatientJournal.Shared.DTOs;
using PatientJournal.Shared.Models;

namespace PatientJournal.Assessment.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http) => _http = http;

    public async Task<List<SessionSummaryDto>> GetSessionsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<SessionSummaryDto>>("api/sessions");
        return result ?? new();
    }

    public async Task<SimulationSession?> GetActiveSessionAsync()
    {
        try { return await _http.GetFromJsonAsync<SimulationSession>("api/sessions/active"); }
        catch { return null; }
    }

    public async Task<List<InterventionDto>> GetInterventionsAsync(int sessionId)
    {
        var result = await _http.GetFromJsonAsync<List<InterventionDto>>($"api/sessions/{sessionId}/interventions");
        return result ?? new();
    }

    public async Task<List<TeacherObservationDto>> GetObservationsAsync(int sessionId)
    {
        var result = await _http.GetFromJsonAsync<List<TeacherObservationDto>>($"api/sessions/{sessionId}/observations");
        return result ?? new();
    }

    public async Task<TeacherObservationDto?> AddObservationAsync(int sessionId, string note)
    {
        var response = await _http.PostAsJsonAsync($"api/sessions/{sessionId}/observations",
            new AddObservationRequest(sessionId, note));
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TeacherObservationDto>();
    }

    public async Task<DebriefReport?> GetDebriefAsync(int sessionId)
    {
        try { return await _http.GetFromJsonAsync<DebriefReport>($"api/sessions/{sessionId}/debrief"); }
        catch { return null; }
    }

    public async Task<bool> EndSessionAsync(int sessionId)
    {
        var response = await _http.PostAsync($"api/sessions/{sessionId}/end", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<SessionSummaryDto?> StartSessionAsync(int caseId)
    {
        var response = await _http.PostAsync($"api/sessions/start?caseId={caseId}", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SessionSummaryDto>();
    }

    public async Task<List<CaseListItem>> GetCasesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<CaseListItem>>("api/cases");
        return result?.Where(c => c.IsActive).ToList() ?? new();
    }
}

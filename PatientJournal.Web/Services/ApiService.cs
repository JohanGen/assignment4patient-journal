using PatientJournal.Shared.DTOs;
using PatientJournal.Shared.Models;
using System.Net.Http.Json;

namespace PatientJournal.Web.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http) => _http = http;

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest(username, password));
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<LoginResponse?> RegisterAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new LoginRequest(username, password));
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<List<CaseListItem>> GetCasesAsync(int userId, string role)
    {
        var result = await _http.GetFromJsonAsync<List<CaseListItem>>($"api/cases?userId={userId}&role={role}");
        return result ?? new();
    }

    public async Task<SimulationCase?> GetCaseAsync(int id)
        => await _http.GetFromJsonAsync<SimulationCase>($"api/cases/{id}");

    public async Task<SimulationCase?> CreateCaseAsync(SimulationCase simCase)
    {
        var response = await _http.PostAsJsonAsync("api/cases", simCase);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SimulationCase>();
    }

    public async Task<bool> UpdateCaseAsync(int id, SimulationCase simCase, int userId, string role)
    {
        var response = await _http.PutAsJsonAsync($"api/cases/{id}?userId={userId}&role={role}", simCase);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ActivateCaseAsync(int id, string role)
    {
        var response = await _http.PostAsync($"api/cases/{id}/activate?role={role}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeactivateCaseAsync(int id, string role)
    {
        var response = await _http.PostAsync($"api/cases/{id}/deactivate?role={role}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCaseAsync(int id, string role)
    {
        var response = await _http.DeleteAsync($"api/cases/{id}?role={role}");
        return response.IsSuccessStatusCode;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientJournal.Shared.Models;
using PatientJournal.Web.Services;
using System.Security.Claims;

namespace PatientJournal.Web.Controllers;

[Authorize]
public class CasesController : Controller
{
    private readonly ApiService _api;

    public CasesController(ApiService api) => _api = api;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string Role => User.FindFirstValue(ClaimTypes.Role)!;

    public async Task<IActionResult> Index()
    {
        var cases = await _api.GetCasesAsync(UserId, Role);
        return View(cases);
    }

    public async Task<IActionResult> Details(int id)
    {
        var simCase = await _api.GetCaseAsync(id);
        if (simCase is null) return NotFound();
        return View(simCase);
    }

    [Authorize(Roles = "Teacher")]
    public IActionResult Create() => View(new SimulationCase());

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create(SimulationCase model, string medications)
    {
        model.CreatedByUserId = UserId;
        model.Medications = ParseMedications(medications);
        var created = await _api.CreateCaseAsync(model);
        if (created is null)
        {
            ModelState.AddModelError("", "Failed to create case.");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var simCase = await _api.GetCaseAsync(id);
        if (simCase is null) return NotFound();
        if (simCase.IsTeacherOnly && Role != "Teacher") return Forbid();
        return View(simCase);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, SimulationCase model, string medications)
    {
        if (model.IsTeacherOnly && Role != "Teacher") return Forbid();
        model.Medications = ParseMedications(medications);
        var success = await _api.UpdateCaseAsync(id, model, UserId, Role);
        if (!success)
        {
            ModelState.AddModelError("", "Failed to update case.");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Activate(int id)
    {
        await _api.ActivateCaseAsync(id, Role);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _api.DeactivateCaseAsync(id, Role);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        await _api.DeleteCaseAsync(id, Role);
        return RedirectToAction(nameof(Index));
    }

    private static List<CaseMedication> ParseMedications(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new();
        var meds = new List<CaseMedication>();
        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|');
            if (parts.Length >= 4)
                meds.Add(new CaseMedication
                {
                    DrugName = parts[0].Trim(),
                    Dosage = parts[1].Trim(),
                    Route = parts[2].Trim(),
                    Frequency = parts[3].Trim()
                });
        }
        return meds;
    }
}

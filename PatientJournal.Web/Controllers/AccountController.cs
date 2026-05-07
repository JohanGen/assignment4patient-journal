using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PatientJournal.Web.Services;
using System.Security.Claims;

namespace PatientJournal.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var result = await _api.LoginAsync(username, password);
        if (result is null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        await SignInAsync(result.UserId, result.Username, result.Role);
        return Redirect(returnUrl ?? Url.Action("Index", "Cases")!);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string username, string password)
    {
        var result = await _api.RegisterAsync(username, password);
        if (result is null)
        {
            ModelState.AddModelError("", "Registration failed. Username may already be taken.");
            return View();
        }

        await SignInAsync(result.UserId, result.Username, result.Role);
        return RedirectToAction("Index", "Cases");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task SignInAsync(int userId, string username, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}

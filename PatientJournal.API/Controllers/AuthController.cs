using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientJournal.API.Data;
using PatientJournal.Shared.DTOs;
using PatientJournal.Shared.Models;
using System.Security.Cryptography;
using System.Text;

namespace PatientJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var hash = HashPassword(request.Password);
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == request.Username && u.PasswordHash == hash);

        if (user is null)
            return Unauthorized(new { message = "Invalid username or password." });

        // Simple token: base64 of userId:role (not production-safe, sufficient for this project)
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{user.Role}"));
        return Ok(new LoginResponse(user.Id, user.Username, user.Role.ToString(), token));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "Username already taken." });

        var user = new User
        {
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            Role = UserRole.Student
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{user.Role}"));
        return Ok(new LoginResponse(user.Id, user.Username, user.Role.ToString(), token));
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "pj_salt"));
        return Convert.ToHexString(bytes);
    }
}

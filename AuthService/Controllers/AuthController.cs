using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    // Демо-хранилище пользователей: пароль, роли и дополнительные claim'ы.
    // BranchId — отдел, в котором работает пользователь (для кастомных политик ResourceService)
    private static readonly Dictionary<string, (string Password, List<string> Roles, List<Claim> Claims)> Users = new()
    {
        ["alice"] = ("secret1", new List<string> { "User" }, new List<Claim> { new Claim("BranchId", "B001") }),
        ["bob"] = ("secret2", new List<string> { "Admin" }, new List<Claim> { new Claim("BranchId", "HQ") }),
        ["charlie"] = ("secret3", new List<string> { "User" }, new List<Claim> { new Claim("BranchId", "B002") })
    };

    public AuthController(TokenService tokenService, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _configuration = configuration;
    }

    // POST /api/auth/login  { "userName": "alice", "password": "secret1" }
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!Users.TryGetValue(request.UserName, out var user))
        {
            return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
        }

        if (user.Password != request.Password)
        {
            return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
        }

        // генерим токен и кладём в него роли и дополнительные claim'ы пользователя
        var token = _tokenService.GenerateToken(request.UserName, user.Roles, user.Claims);
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireInMinutes"] ?? "60");

        return Ok(new
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(expireMinutes).ToString("o")
        });
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using AuthCookie.Models;

namespace AuthCookie.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        // POST /auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = Users.Find(request.UserName);

            if (user is null || user.Password != request.Password)
            {
                return Unauthorized(new { error = "Неверное имя пользователя или пароль" });
            }

            // Claims — данные, которые попадут в cookie (сигнируются, но не шифруются)
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Role)
            };

            // Дополнительный claim для демонстрации кастомной политики
            if (user.Department is not null)
            {
                claims.Add(new Claim("department", user.Department));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            // Сервер генерирует cookie, клиент получает её в Set-Cookie и шлёт автоматически
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);

            return Ok(new { message = $"Добро пожаловать, {user.UserName}!", role = user.Role });
        }

        // GET /auth/me
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                name = User.Identity?.Name,
                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }

        // POST /auth/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Вы завершили сессию" });
        }
    }
}

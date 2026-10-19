using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthCookie.Policies;

namespace AuthCookie.Controllers
{
    /// <summary>
    /// Демонстрация JWT-аутентификации: токены выпускает AuthService
    /// (POST {AuthService}/api/auth/login) и валидируются здесь
    /// по общему секрету подписи (секция Jwt в appsettings.json).
    /// </summary>
    [ApiController]
    [Route("api/bearer")]
    public class BearerAuthController : ControllerBase
    {
        /// <summary>
        /// Точка входа: запрашивает JWT из AuthService и возвращает его.
        /// Заголовок X-Forward-Base-Url — базовый URL AuthService
        /// (по умолчанию https://localhost:7208 — профиль https из launchSettings).
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromQuery] string userName,
            [FromQuery] string password,
            [FromHeader(Name = "X-Forward-Base-Url")] string? forwardBaseUrl = null)
        {
            forwardBaseUrl ??= "https://localhost:7208";

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync(
                $"{forwardBaseUrl.TrimEnd('/')}/api/auth/login",
                new { userName, password });

            // Прокидываем статус AuthService как есть (401 при неверном пароле)
            if (!response.IsSuccessStatusCode)
            {
                return new ObjectResult(await response.Content.ReadAsStringAsync())
                {
                    StatusCode = (int)response.StatusCode
                };
            }

            // { "Token": "...", "Expiration": "..." }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return Ok(result);
        }

        /// <summary>
        /// Доступен только по валидному JWT из AuthService:
        /// Authorization: Bearer &lt;token&gt;
        /// Cookie здесь не подходит — схема явно указана Bearer.
        /// </summary>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Me()
        {
            return Ok(new
            {
                sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"),
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value),
                branchId = User.FindFirstValue("BranchId")
            });
        }

        /// <summary>
        /// JWT + политика: доступен только токену с BranchId = "B001"
        /// (в демо AuthService это alice).
        /// </summary>
        [HttpGet("branch-books")]
        [Authorize(Policy = PolicyNames.CanViewBranchB001,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetBranchBooks()
        {
            return Ok(new
            {
                message = "Книги отдела B001",
                branchId = User.FindFirstValue("BranchId")
            });
        }
    }
}

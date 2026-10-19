using AuthCookie.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthCookie.Controllers
{
    [ApiController]
    [Route("secure")]
    public class SecureController : ControllerBase
    {
        // GET /secure/data — доступен любому авторизованному пользователю
        [Authorize]
        [HttpGet("data")]
        public IActionResult GetData()
        {
            return Ok(new
            {
                message = "Защищённые данные",
                user = User.Identity?.Name,
                authenticated = User.Identity?.IsAuthenticated
            });
        }

        // GET /secure/admin — только для роли Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminData()
        {
            return Ok(new
            {
                message = "Данные панели администратора",
                user = User.Identity?.Name
            });
        }

        /// <summary>
        /// Демонстрация авторизации на основе политик.
        /// Метод защищён кастомной политикой "FinanceDepartment":
        /// для доступа нужно и быть авторизованным, и иметь claim department=finance.
        /// Проверку выполняет FinanceDepartmentHandler (см. Policies/).
        /// </summary>
        // GET /auth/finance-report
        [Authorize(Policy = PolicyNames.FinanceDepartment)]
        [HttpGet("finance-report")]
        public IActionResult GetFinanceReport()
        {
            return Ok(new
            {
                message = "Финансовый отчёт за квартал",
                user = User.Identity?.Name,
                department = User.FindFirstValue("department")
            });
        }
    }
}

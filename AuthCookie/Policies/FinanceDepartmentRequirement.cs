using Microsoft.AspNetCore.Authorization;

namespace AuthCookie.Policies
{
    /// <summary>
    /// Требование для политики: пользователь принадлежит к финансовому отделу.
    /// Класс без состояния — singleton, состояние хранится в claim.
    /// </summary>
    public class FinanceDepartmentRequirement : IAuthorizationRequirement
    {
    }
}

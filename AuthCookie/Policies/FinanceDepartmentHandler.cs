using Microsoft.AspNetCore.Authorization;

namespace AuthCookie.Policies
{
    /// <summary>
    /// Проверяет, есть ли у пользователя claim "department" со значением "finance".
    /// </summary>
    public class FinanceDepartmentHandler : AuthorizationHandler<FinanceDepartmentRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            FinanceDepartmentRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == "department" && c.Value == "finance"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

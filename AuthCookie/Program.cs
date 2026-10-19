using AuthCookie.Policies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // cookie уходит только по HTTPS

        options.LoginPath = "/auth/login";
        options.AccessDeniedPath = "/auth/login";
        options.LogoutPath = "/auth/logout";

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true; // cookie продлевается при активности

        // Событие для отладки: видно, когда авторизация срабатывает
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.WriteAsJsonAsync(new { error = "Требуется аутентификация" });
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.WriteAsJsonAsync(new { error = "Доступ запрещён" });
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Политика "FinanceDepartment": требует claim department=finance
    options.AddPolicy(PolicyNames.FinanceDepartment, policy =>
        policy.AddRequirements(new FinanceDepartmentRequirement()));
});

// Регистрация кастомного обработчика требований
builder.Services.AddSingleton<IAuthorizationHandler, FinanceDepartmentHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

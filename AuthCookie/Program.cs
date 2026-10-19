using AuthCookie.Policies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Кнопка "Authorize" в Swagger: вставляем сюда JWT, полученный из AuthService
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставьте JWT-токен, полученный от AuthService /api/auth/login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwt = builder.Configuration.GetSection("Jwt");

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
    })
    // Вторая схема аутентификации: JWT, выпущенный AuthService (общий секрет подписи).
    // Cookie остаётся основной схемой: [Authorize] без явной схемы проверяет её,
    // JWT требуется только там, где явно указан Scheme = JwtBearerDefaults.AuthenticationScheme.
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwt["Secret"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Политика "FinanceDepartment": требует claim department=finance
    options.AddPolicy(PolicyNames.FinanceDepartment, policy =>
        policy.AddRequirements(new FinanceDepartmentRequirement()));

    // Политика на claim'ы из JWT AuthService: видит книги отдела только BranchId=B001 (alice)
    options.AddPolicy(PolicyNames.CanViewBranchB001, policy =>
        policy.RequireClaim("BranchId", "B001"));
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

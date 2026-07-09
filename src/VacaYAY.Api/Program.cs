using System.Security.Claims;
using System.Text;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VacaYAY.Business.DTOs.Auth;
using VacaYAY.Business.Interfaces.Auth;
using VacaYAY.Business.Services.Auth;
using VacaYAY.Business.Validators.Auth;
using VacaYAY.Data;
using VacaYAY.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<VacaYAYDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwt.SigningKey) || Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is missing or shorter than 32 bytes. Set it in user-secrets: " +
        "dotnet user-secrets set \"Jwt:SigningKey\" \"<32+ byte random value>\"");
}

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenDenylist, TokenDenylist>(); // singleton: revocations must outlive requests

// FluentValidation: register every validator in the Business assembly.

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordRequestValidator>();


// Mapster: run every IRegister (e.g. AuthMappingConfig) into the global config.
TypeAdapterConfig.GlobalSettings.Scan(typeof(IAuthService).Assembly);

// ---- Authentication (JWT bearer) ----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep original claim names ("sub", "role") instead of remapping to long URIs.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.Sub,
        };

        // Reject tokens that were revoked via logout, even if still within their lifetime.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var denylist = context.HttpContext.RequestServices.GetRequiredService<ITokenDenylist>();
                var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (jti is not null && denylist.IsRevoked(jti))
                {
                    context.Fail("This token has been revoked.");
                }

                return Task.CompletedTask;
            },
        };
    });

// ---- Authorization ----
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HrOnly", policy => policy.RequireRole(nameof(UserRole.HR)));
});

var app = builder.Build();

// ---- HTTP pipeline ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();

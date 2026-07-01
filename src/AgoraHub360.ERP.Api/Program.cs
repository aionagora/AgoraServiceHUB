using System.Text;
using System.Globalization;
using AgoraHub360.ERP.Api.Auth;
using AgoraHub360.ERP.Api.Authorization.Handlers;
using AgoraHub360.ERP.Api.Authorization.Requirements;
using AgoraHub360.ERP.Api.BackgroundServices;
using AgoraHub360.ERP.Api.Middleware;
using AgoraHub360.ERP.Api.Services;
using AgoraHub360.ERP.Application;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Infrastructure;
using AgoraHub360.ERP.Persistence;
using AgoraHub360.ERP.Persistence.Context;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.Configuration;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var numericCulture = AppFormattingOptions.BuildNumericCulture();
var uiCulture = AppFormattingOptions.BuildUiCulture();
CultureInfo.DefaultThreadCurrentCulture = numericCulture;
CultureInfo.DefaultThreadCurrentUICulture = uiCulture;

// ──── Servicios de infraestructura HTTP ────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ──── Capas Clean Architecture ────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddApplication();
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure();

// ──── Autenticación JWT + Stub (desarrollo) ────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "AgoraHub360-ERP-Dev-Secret-Key-2026-MinLength32!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AgoraHub360.ERP";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AgoraHub360.ERP.Web";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
})
.AddScheme<AuthenticationSchemeOptions, StubAuthHandler>(
    StubAuthHandler.SchemeName, _ => { });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthorizationHandler, TenantMembershipHandler>();
builder.Services.AddScoped<IAuthorizationHandler, BranchAccessHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.RequireAuthenticated, policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy(PolicyNames.RequirePlatformSuperAdmin, policy =>
        policy.RequireAssertion(context =>
        {
            var role = context.User.FindFirst(ClaimTypesCustom.PlatformRole)?.Value
                       ?? context.User.FindFirst("PlatformRole")?.Value
                       ?? context.User.FindFirst("platformRole")?.Value
                       ?? context.User.FindFirst("platform_role")?.Value;
            return string.Equals(role, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);
        }));

    options.AddPolicy(PolicyNames.RequirePlatformAdmin, policy =>
        policy.RequireAssertion(context =>
        {
            var role = context.User.FindFirst(ClaimTypesCustom.PlatformRole)?.Value
                       ?? context.User.FindFirst("PlatformRole")?.Value
                       ?? context.User.FindFirst("platformRole")?.Value
                       ?? context.User.FindFirst("platform_role")?.Value;
            return string.Equals(role, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);
        }));

    options.AddPolicy(PolicyNames.RequireTenantSelected, policy =>
        policy.RequireAssertion(context =>
        {
            var status = context.User.FindFirst(ClaimTypesCustom.TenantStatus)?.Value;
            var tenantIdValue = context.User.FindFirst(ClaimTypesCustom.TenantId)?.Value
                                ?? context.User.FindFirst(ClaimTypesCustom.EmpresaId)?.Value;

            return string.Equals(status, TenantStatus.Selected, StringComparison.OrdinalIgnoreCase)
                   && int.TryParse(tenantIdValue, out var tenantId)
                   && tenantId > 0;
        }));

    options.AddPolicy(PolicyNames.RequireTenantMembership, policy =>
        policy.Requirements.Add(new TenantMembershipRequirement()));

    options.AddPolicy(PolicyNames.RequireTenantAdmin, policy =>
        policy.RequireAssertion(context =>
        {
            var role = context.User.FindFirst(ClaimTypesCustom.TenantRole)?.Value;
            return string.Equals(role, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase);
        }));

    options.AddPolicy(PolicyNames.RequireTenantSupervisor, policy =>
        policy.RequireAssertion(context =>
        {
            var role = context.User.FindFirst(ClaimTypesCustom.TenantRole)?.Value;
            return string.Equals(role, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.Supervisor, StringComparison.OrdinalIgnoreCase);
        }));

    options.AddPolicy(PolicyNames.RequireTenantOperator, policy =>
        policy.RequireAssertion(context =>
        {
            var role = context.User.FindFirst(ClaimTypesCustom.TenantRole)?.Value;
            return string.Equals(role, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.Supervisor, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.Operador, StringComparison.OrdinalIgnoreCase);
        }));

    options.AddPolicy(PolicyNames.RequireBranchAccessRead, policy =>
        policy.Requirements.Add(new BranchAccessRequirement(canOperate: false)));

    options.AddPolicy(PolicyNames.RequireBranchAccessOperate, policy =>
        policy.Requirements.Add(new BranchAccessRequirement(canOperate: true)));

    options.AddPolicy(PolicyNames.RequireAuditGlobalRead, policy =>
        policy.RequireAssertion(context =>
        {
            var role = context.User.FindFirst(ClaimTypesCustom.PlatformRole)?.Value;
            return string.Equals(role, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role, Roles.SecurityAuditor, StringComparison.OrdinalIgnoreCase);
        }));
});

// ──── API Versioning ────
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// ──── Controllers ────
builder.Services.AddControllers();

// ──── Swagger / OpenAPI ────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgoraHub360 ERP API",
        Version = "v1.0.0",
        Description = "API del sistema ERP multi-empresa AgoraHub360 — Gestión 2026",
        Contact = new OpenApiContact
        {
            Name = "AgoraHub360",
            Email = "soporte@agorahub360.com"
        }
    });

    // Soporte para autenticación en Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token. En modo stub, cualquier valor activa la autenticación."
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

// ──── CORS (Blazor WASM) ────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorDev", policy =>
    {
        var origins = new List<string>
        {
            "http://localhost:5001",
            "https://localhost:5002"
        };

        var blazorBaseUrl = builder.Configuration.GetValue<string>("BlazorBaseUrl");
        if (!string.IsNullOrEmpty(blazorBaseUrl) && !origins.Contains(blazorBaseUrl))
            origins.Add(blazorBaseUrl);

        // Orígenes adicionales desde variable de entorno o configuración
        var corsOrigins = builder.Configuration.GetValue<string>("CorsOrigins");
        if (!string.IsNullOrEmpty(corsOrigins))
        {
            foreach (var origin in corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!origins.Contains(origin))
                    origins.Add(origin);
            }
        }

        policy
            .WithOrigins(origins.Distinct().ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ──── Health Checks ────
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AgoraDbContext>("database");

// ──── Background Services ────
builder.Services.AddHostedService<PeriodoContableNotificadorService>();

// ──── Configuración de QuestPDF ────
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ════════════════════════════════════════════
var app = builder.Build();
// ════════════════════════════════════════════

// Middleware de errores primero (captura todo)
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AgoraHub360 ERP API v1");
        options.DocumentTitle = "AgoraHub360 ERP - Swagger";
    });
}

// CORS debe ir ANTES que HttpsRedirection para que las preflight requests
// (OPTIONS) reciban los headers CORS antes de cualquier redirect.
app.UseCors("AllowBlazorDev");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Validacion tenant: operaciones de escritura requieren EmpresaId
app.UseMiddleware<TenantRequiredMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

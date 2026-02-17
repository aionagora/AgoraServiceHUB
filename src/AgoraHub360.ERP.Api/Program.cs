using AgoraHub360.ERP.Api.Auth;
using AgoraHub360.ERP.Api.Middleware;
using AgoraHub360.ERP.Api.Services;
using AgoraHub360.ERP.Application;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Infrastructure;
using AgoraHub360.ERP.Persistence;
using AgoraHub360.ERP.Persistence.Context;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ──── Servicios de infraestructura HTTP ────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ──── Capas Clean Architecture ────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddApplication();
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure();

// ──── Autenticación Stub (desarrollo) ────
builder.Services.AddAuthentication(StubAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, StubAuthHandler>(
        StubAuthHandler.SchemeName, _ => { });

builder.Services.AddAuthorization();

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
        Version = "v1",
        Description = "API del sistema ERP multi-empresa AgoraHub360",
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

// ──── Health Checks ────
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AgoraDbContext>("database");

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

using AgoraHub360.ERP.Api.Services;
using AgoraHub360.ERP.Application;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Infrastructure;
using AgoraHub360.ERP.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Servicios de infraestructura HTTP
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Capas Clean Architecture
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddApplication();
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure();

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

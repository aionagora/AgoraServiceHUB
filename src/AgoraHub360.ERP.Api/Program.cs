using AgoraHub360.ERP.Application;
using AgoraHub360.ERP.Infrastructure;
using AgoraHub360.ERP.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Capas Clean Architecture
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=AgoraHub360;Trusted_Connection=true;TrustServerCertificate=true;";

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

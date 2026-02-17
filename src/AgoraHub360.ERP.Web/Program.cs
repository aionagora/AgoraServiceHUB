using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AgoraHub360.ERP.Web;
using AgoraHub360.ERP.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient configurado para la API
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl")
    ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl),
    DefaultRequestHeaders =
    {
        { "Authorization", "Bearer stub-token" }
    }
});

// Servicios
builder.Services.AddScoped<EmpresaHttpService>();
builder.Services.AddScoped<EmpresaStateService>();
builder.Services.AddScoped<RolHttpService>();
builder.Services.AddScoped<UsuarioHttpService>();
builder.Services.AddScoped<AuditLogHttpService>();

await builder.Build().RunAsync();

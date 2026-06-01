using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AgoraHub360.ERP.Web;
using AgoraHub360.ERP.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient configurado para la API (sin token hardcodeado; JwtAuthStateProvider lo inyecta)
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl")
    ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// ──── Autenticación ────
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

// ──── Servicios HTTP ────
builder.Services.AddScoped<FileDownloadService>();
builder.Services.AddScoped<AuthHttpService>();
builder.Services.AddScoped<EmpresaHttpService>();
builder.Services.AddScoped<EmpresaStateService>();
builder.Services.AddScoped<HttpSucursalService>();
builder.Services.AddScoped<GeoCatalogHttpService>();
builder.Services.AddScoped<RolHttpService>();
builder.Services.AddScoped<UsuarioHttpService>();
builder.Services.AddScoped<AuditLogHttpService>();
builder.Services.AddScoped<ParametroHttpService>();
builder.Services.AddScoped<NumeracionHttpService>();
builder.Services.AddScoped<CierreContableHttpService>();

// ──── Servicios HTTP MDM (legacy) ────
builder.Services.AddScoped<CategoriaProductoHttpService>();
builder.Services.AddScoped<CatalogoHttpService>();
builder.Services.AddScoped<UnidadMedidaHttpService>();
builder.Services.AddScoped<ProductoHttpService>();
builder.Services.AddScoped<ClienteHttpService>();
builder.Services.AddScoped<ClientePerfilFiscalHttpService>();
builder.Services.AddScoped<ClienteSucursalHttpService>();
builder.Services.AddScoped<ProveedorHttpService>();
builder.Services.AddScoped<AlmacenHttpService>();

// ──── Servicios HTTP MDM avanzado (Gestion de Productos) ────
builder.Services.AddScoped<MdmProductoGlobalHttpService>();
builder.Services.AddScoped<MdmCompanyProductHttpService>();
builder.Services.AddScoped<MdmVariantHttpService>();
builder.Services.AddScoped<MdmAttributeHttpService>();
builder.Services.AddScoped<BrandHttpService>();
builder.Services.AddScoped<ManufacturerHttpService>();
builder.Services.AddScoped<ProductUomHttpService>();
builder.Services.AddScoped<ProductCodeHttpService>();
builder.Services.AddScoped<PriceListHttpService>();

// ──── Servicios HTTP Inventario ────
builder.Services.AddScoped<MovimientoInventarioHttpService>();

// ──── Servicios HTTP Contabilidad ────
builder.Services.AddScoped<CuentaContableHttpService>();
builder.Services.AddScoped<AsientoContableHttpService>();
builder.Services.AddScoped<PeriodoContableHttpService>();
builder.Services.AddScoped<PlantillaContableHttpService>();
builder.Services.AddScoped<EstadoFinancieroHttpService>();
builder.Services.AddScoped<HttpExportEstadosFinancierosService>();
builder.Services.AddScoped<CentroCostoHttpService>();
builder.Services.AddScoped<HttpFlujoDEfectivoService>();
builder.Services.AddScoped<HttpLibroMayorService>();

// ──── Servicios HTTP Compras ────
builder.Services.AddScoped<OrdenPedidoHttpService>();
builder.Services.AddScoped<OrdenCompraHttpService>();
builder.Services.AddScoped<RecepcionCompraHttpService>();
builder.Services.AddScoped<HojaImportacionHttpService>();
builder.Services.AddScoped<ExpedienteImportacionHttpService>();

// Ventas
builder.Services.AddScoped<PedidoVentaHttpService>();
builder.Services.AddScoped<VentaHttpService>();
builder.Services.AddScoped<FacturaVentaHttpService>();

// ──── Servicios HTTP Logística ────
builder.Services.AddScoped<HojaRutaHttpService>();

// ──── Servicios HTTP Workflow ────
builder.Services.AddScoped<IWorkflowClientService, WorkflowHttpService>();

// ──── Dashboard ────
builder.Services.AddScoped<DashboardDataService>();
builder.Services.AddScoped<SeguridadDinamicaHttpService>();
builder.Services.AddScoped<SesionUsuarioStateService>();
builder.Services.AddScoped<UiAuthorizationService>();

// ──── Demo Seed ────
builder.Services.AddScoped<EmpresaDemoService>();

await builder.Build().RunAsync();

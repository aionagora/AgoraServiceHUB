using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AgoraHub360.ERP.Web;
using AgoraHub360.ERP.Web.Services;
using AgoraHub360.ERP.Web.Theme;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;

// ── Cultura global: punto decimal, coma miles (toda la app Blazor WASM) ──
var fixedCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = fixedCulture;
CultureInfo.DefaultThreadCurrentUICulture = fixedCulture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient configurado para la API (sin token hardcodeado; JwtAuthStateProvider lo inyecta)
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl")
    ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddTransient<AuthMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

// ──── Autenticación ────
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

// ──── Servicios HTTP ────
builder.Services.AddScoped<FileDownloadService>();
builder.Services.AddScoped<PdfDownloadService>();
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
builder.Services.AddScoped<CuentasPorCobrarHttpService>();
builder.Services.AddScoped<ClienteCreditoConfiguracionHttpService>();

// FE: Facturación Electrónica
builder.Services.AddScoped<FacturacionFEHttpService>();

// ──── Servicios HTTP Logística ────
builder.Services.AddScoped<HojaRutaHttpService>();

// ──── Servicios HTTP Workflow ────
builder.Services.AddScoped<IWorkflowClientService, WorkflowHttpService>();

// ──── Dashboard ────
builder.Services.AddScoped<DashboardDataService>();
builder.Services.AddScoped<SeguridadDinamicaHttpService>();
builder.Services.AddScoped<SesionUsuarioStateService>();
builder.Services.AddScoped<UiAuthorizationService>();

// ──── MudBlazor ────
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
});

// ──── Demo Seed ────
builder.Services.AddScoped<EmpresaDemoService>();

await builder.Build().RunAsync();

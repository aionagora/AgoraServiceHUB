namespace AgoraHub360.ERP.Application;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Application.Validators.Workflow;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MemoryCache for plan de cuentas and similar catalog structures
        services.AddMemoryCache();

        // ?? Validadores FluentValidation??????????????????????????????????????
        // Registra todos los AbstractValidator<T> del assembly Application.
        services.AddValidatorsFromAssemblyContaining<TareaCreateValidator>();

        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<ISucursalService, SucursalService>();
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IParametroSistemaService, ParametroSistemaService>();
        services.AddScoped<INumeracionDocumentoService, NumeracionDocumentoService>();

        // MDM: third parties
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<IAlmacenService, AlmacenService>();

        // MDM: unified catalog
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IUomService, UomService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICompanyProductService, CompanyProductService>();
        services.AddScoped<IProductCodeService, ProductCodeService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAttributeDefinitionService, AttributeDefinitionService>();
        services.AddScoped<IProductVariantService, ProductVariantService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IManufacturerService, ManufacturerService>();
        services.AddScoped<IProductUomService, ProductUomService>();

        // Legacy adapters (keep controllers/pages compiling)
        services.AddScoped<IUnidadMedidaService, UnidadMedidaService>();
        services.AddScoped<ICategoriaProductoService, CategoriaProductoService>();
        services.AddScoped<IProductoService, ProductoService>();

        // RUL
        services.AddScoped<IIndustryService, IndustryService>();

        // VER
        services.AddScoped<IVersioningService, VersioningService>();

        // PRC
        services.AddScoped<IPriceListService, PriceListService>();

        // INV
        services.AddScoped<IMovimientoInventarioService, MovimientoInventarioService>();

        // CMP
        services.AddScoped<IOrdenPedidoService, OrdenPedidoService>();
        services.AddScoped<IOrdenCompraService, OrdenCompraService>();
        services.AddScoped<IRecepcionCompraService, RecepcionCompraService>();
        services.AddScoped<IHojaImportacionService, HojaImportacionService>();
        services.AddScoped<IExpedienteImportacionService, ExpedienteImportacionService>();

        // ACC
        services.AddScoped<ICuentaContableService, CuentaContableService>();
        services.AddScoped<IAsientoContableService, AsientoContableService>();
        services.AddScoped<IPeriodoContableService, PeriodoContableService>();
        services.AddScoped<IPlantillaContableService, PlantillaContableService>();
        services.AddScoped<IContabilizacionService, ContabilizacionService>();
        services.AddScoped<IEstadoFinancieroService, EstadoFinancieroService>();
        services.AddScoped<ICentroCostoService, CentroCostoService>();
        services.AddScoped<IComprobanteDocumentoService, ComprobanteDocumentoService>();
        services.AddScoped<ICierreContableService, CierreContableService>();

        // Notificaciones
        services.AddScoped<INotificacionService, LogNotificacionService>();
        services.AddScoped<IPeriodoContableNotificacionService, PeriodoContableNotificacionService>();

        // TRB: Tributario
        services.AddScoped<IImpuestoService, ImpuestoService>();

        // ACT: Activos Fijos
        services.AddScoped<IActivoFijoService, ActivoFijoService>();

        // ACC: Presupuestos
        services.AddScoped<IPresupuestoService, PresupuestoService>();

        // BNC: Conciliación Bancaria
        services.AddScoped<IConciliacionBancariaService, ConciliacionBancariaService>();

        // WF: Workflow
        services.AddScoped<IWorkflowService, WorkflowService>();

        return services;
    }
}

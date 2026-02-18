namespace AgoraHub360.ERP.Application;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registro de servicios de la capa Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IParametroSistemaService, ParametroSistemaService>();
        services.AddScoped<INumeracionDocumentoService, NumeracionDocumentoService>();

        // MDM legacy
        services.AddScoped<ICategoriaProductoService, CategoriaProductoService>();
        services.AddScoped<IUnidadMedidaService, UnidadMedidaService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<IAlmacenService, AlmacenService>();

        // MDM nuevo modelo
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICompanyProductService, CompanyProductService>();
        services.AddScoped<IProductCodeService, ProductCodeService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAttributeDefinitionService, AttributeDefinitionService>();
        services.AddScoped<IProductVariantService, ProductVariantService>();

        // RUL
        services.AddScoped<IIndustryService, IndustryService>();

        // VER
        services.AddScoped<IVersioningService, VersioningService>();

        // PRC
        services.AddScoped<IPriceListService, PriceListService>();

        return services;
    }
}

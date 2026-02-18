namespace AgoraHub360.ERP.Persistence.Context;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.CST;
using AgoraHub360.ERP.Domain.Entities.DOC;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.PRC;
using AgoraHub360.ERP.Domain.Entities.RUL;
using AgoraHub360.ERP.Domain.Entities.VER;
using AgoraHub360.ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// DbContext principal del ERP con soporte multi-tenant y auditoría automática.
/// Los filtros globales de tenant garantizan aislamiento de datos por empresa.
/// </summary>
public class AgoraDbContext : DbContext, IUnitOfWork
{
    private readonly int? _empresaId;

    public AgoraDbContext(
        DbContextOptions<AgoraDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _empresaId = currentUserService.EmpresaId;
    }

    // Constructor para migraciones y design-time (sin tenant)
    public AgoraDbContext(DbContextOptions<AgoraDbContext> options)
        : base(options)
    {
    }

    // ── Core ──────────────────────────────────────────────────────────────────
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioEmpresa> UsuarioEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<Rol> Roles => Set<Rol>();

    // Configuración
    public DbSet<ParametroSistema> ParametrosSistema => Set<ParametroSistema>();
    public DbSet<NumeracionDocumento> NumeracionesDocumento => Set<NumeracionDocumento>();

    // ── MDM Legacy (mantener compatibilidad) ─────────────────────────────────
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Almacen> Almacenes => Set<Almacen>();
    public DbSet<UbicacionAlmacen> UbicacionesAlmacen => Set<UbicacionAlmacen>();

    // ── MDM: Catálogo y producto base (nuevo modelo) ──────────────────────────
    public DbSet<Catalog> Catalogs => Set<Catalog>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<ProductStatus> ProductStatuses => Set<ProductStatus>();
    public DbSet<Uom> Uoms => Set<Uom>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CompanyProduct> CompanyProducts => Set<CompanyProduct>();
    public DbSet<ProductCode> ProductCodes => Set<ProductCode>();

    // ── MDM: Clasificación ────────────────────────────────────────────────────
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductClassification> ProductClassifications => Set<ProductClassification>();
    public DbSet<ProductClassificationLink> ProductClassificationLinks => Set<ProductClassificationLink>();

    // ── MDM: Atributos dinámicos ──────────────────────────────────────────────
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeOption> AttributeOptions => Set<AttributeOption>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();

    // ── MDM: Variantes y presentaciones ──────────────────────────────────────
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<VariantAttributeValue> VariantAttributeValues => Set<VariantAttributeValue>();
    public DbSet<ProductUom> ProductUoms => Set<ProductUom>();

    // ── MDM: Features activables ──────────────────────────────────────────────
    public DbSet<CompanyProductFeature> CompanyProductFeatures => Set<CompanyProductFeature>();

    // ── RUL: Reglas por industria ─────────────────────────────────────────────
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<ProductIndustryRule> ProductIndustryRules => Set<ProductIndustryRule>();

    // ── VER: Versionado ───────────────────────────────────────────────────────
    public DbSet<EntityVersion> EntityVersions => Set<EntityVersion>();

    // ── PRC: Precios ──────────────────────────────────────────────────────────
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();

    // ── CST: Costos ───────────────────────────────────────────────────────────
    public DbSet<CostingRule> CostingRules => Set<CostingRule>();
    public DbSet<LandedCostProfile> LandedCostProfiles => Set<LandedCostProfile>();

    // ── DOC: Documentos multimedia ────────────────────────────────────────────
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();

    // ── Auditoría ─────────────────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones Fluent API desde el assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgoraDbContext).Assembly);

        // Filtro global multi-tenant: todas las entidades que heredan de TenantEntity
        // se filtran automáticamente por EmpresaId del usuario actual
        ApplyTenantQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(AgoraDbContext)
                .GetMethod(nameof(ApplyTenantFilter),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, new object[] { modelBuilder });
        }
    }

    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : TenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => _empresaId == null || e.EmpresaId == _empresaId);
        modelBuilder.Entity<T>().HasIndex(e => e.EmpresaId);
    }
}

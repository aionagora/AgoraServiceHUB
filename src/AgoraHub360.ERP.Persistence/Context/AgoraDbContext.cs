namespace AgoraHub360.ERP.Persistence.Context;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.CMP;
using AgoraHub360.ERP.Domain.Entities.CST;
using AgoraHub360.ERP.Domain.Entities.DOC;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.INV;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.PRC;
using AgoraHub360.ERP.Domain.Entities.RUL;
using AgoraHub360.ERP.Domain.Entities.VER;
using AgoraHub360.ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Main ERP DbContext with multi-tenant support and automatic auditing.
/// Global tenant filters guarantee data isolation per company.
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

    // Constructor for migrations and design-time (no tenant)
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
    public DbSet<ParametroSistema> ParametrosSistema => Set<ParametroSistema>();
    public DbSet<NumeracionDocumento> NumeracionesDocumento => Set<NumeracionDocumento>();

    // ── MDM: Third parties ────────────────────────────────────────────────────
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Almacen> Almacenes => Set<Almacen>();
    public DbSet<UbicacionAlmacen> UbicacionesAlmacen => Set<UbicacionAlmacen>();

    // ── MDM: Catalog and base product ─────────────────────────────────────────
    public DbSet<Catalog> Catalogs => Set<Catalog>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<ProductStatus> ProductStatuses => Set<ProductStatus>();
    public DbSet<Uom> Uoms => Set<Uom>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CompanyProduct> CompanyProducts => Set<CompanyProduct>();
    public DbSet<ProductCode> ProductCodes => Set<ProductCode>();

    // ── MDM: Classification ───────────────────────────────────────────────────
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductClassification> ProductClassifications => Set<ProductClassification>();
    public DbSet<ProductClassificationLink> ProductClassificationLinks => Set<ProductClassificationLink>();

    // ── MDM: Dynamic attributes ───────────────────────────────────────────────
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeOption> AttributeOptions => Set<AttributeOption>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();

    // ── MDM: Variants and presentations ──────────────────────────────────────
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<VariantAttributeValue> VariantAttributeValues => Set<VariantAttributeValue>();
    public DbSet<ProductUom> ProductUoms => Set<ProductUom>();

    // ── MDM: Activatable features ─────────────────────────────────────────────
    public DbSet<CompanyProductFeature> CompanyProductFeatures => Set<CompanyProductFeature>();

    // ── RUL: Industry rules ───────────────────────────────────────────────────
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<ProductIndustryRule> ProductIndustryRules => Set<ProductIndustryRule>();

    // ── VER: Versioning ───────────────────────────────────────────────────────
    public DbSet<EntityVersion> EntityVersions => Set<EntityVersion>();

    // ── PRC: Pricing ──────────────────────────────────────────────────────────
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();

    // ── CST: Costing ──────────────────────────────────────────────────────────
    public DbSet<CostingRule> CostingRules => Set<CostingRule>();
    public DbSet<LandedCostProfile> LandedCostProfiles => Set<LandedCostProfile>();

    // ── DOC: Media documents ──────────────────────────────────────────────────
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();

    // ── INV: Inventory ────────────────────────────────────────────────────────
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<StockProducto> StockProductos => Set<StockProducto>();

    // ── CMP: Compras ──────────────────────────────────────────────────────────
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraLinea> OrdenCompraLineas => Set<OrdenCompraLinea>();
    public DbSet<RecepcionCompra> RecepcionesCompra => Set<RecepcionCompra>();
    public DbSet<RecepcionCompraLinea> RecepcionCompraLineas => Set<RecepcionCompraLinea>();
    public DbSet<HojaImportacion> HojasImportacion => Set<HojaImportacion>();
    public DbSet<GastoImportacion> GastosImportacion => Set<GastoImportacion>();
    public DbSet<ImportacionLinea> ImportacionLineas => Set<ImportacionLinea>();

    // ── ACC: Contabilidad ────────────────────────────────────────────────────
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();
    public DbSet<AsientoContable> AsientosContables => Set<AsientoContable>();
    public DbSet<AsientoContableLinea> AsientoContableLineas => Set<AsientoContableLinea>();
    public DbSet<PeriodoContable> PeriodosContables => Set<PeriodoContable>();
    public DbSet<PlantillaContable> PlantillasContables => Set<PlantillaContable>();
    public DbSet<PlantillaContableLinea> PlantillaContableLineas => Set<PlantillaContableLinea>();

    // ── Audit ─────────────────────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Fluent API configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgoraDbContext).Assembly);

        // Global multi-tenant filter: all TenantEntity-derived entities
        // are automatically filtered by the current user's EmpresaId
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

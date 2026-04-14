namespace AgoraHub360.ERP.Persistence.Context;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.CMP;
using AgoraHub360.ERP.Domain.Entities.CST;
using AgoraHub360.ERP.Domain.Entities.DOC;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.INV;
using AgoraHub360.ERP.Domain.Entities.LOG;
using AgoraHub360.ERP.Domain.Entities.Workflow;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.PRC;
using AgoraHub360.ERP.Domain.Entities.RUL;
using AgoraHub360.ERP.Domain.Entities.VER;
using AgoraHub360.ERP.Domain.Entities.TRB;
using AgoraHub360.ERP.Domain.Entities.ACT;
using AgoraHub360.ERP.Domain.Entities.BNC;
using AgoraHub360.ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Main ERP DbContext with multi-tenant support and automatic auditing.
/// Global tenant filters guarantee data isolation per company.
/// </summary>
public class AgoraDbContext : DbContext, IUnitOfWork
{
    private readonly int? _empresaId;
    private IDbContextTransaction? _currentTransaction;

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
    public DbSet<CentroCosto> CentrosCosto => Set<CentroCosto>();

    // ── DOC: Media documents ──────────────────────────────────────────────────
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();
    public DbSet<ComprobanteDocumento> ComprobanteDocumentos => Set<ComprobanteDocumento>();

    // ── INV: Inventory ────────────────────────────────────────────────────────
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<StockProducto> StockProductos => Set<StockProducto>();

    // ── CMP: Compras ──────────────────────────────────────────────────────────
    public DbSet<OrdenPedido> OrdenesPedido => Set<OrdenPedido>();
    public DbSet<OrdenPedidoLinea> OrdenPedidoLineas => Set<OrdenPedidoLinea>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraLinea> OrdenCompraLineas => Set<OrdenCompraLinea>();
    public DbSet<ConfirmacionProveedor> ConfirmacionesProveedor => Set<ConfirmacionProveedor>();
    public DbSet<PagoOrdenCompra> PagosOrdenCompra => Set<PagoOrdenCompra>();
    public DbSet<ExpedienteImportacion> ExpedientesImportacion => Set<ExpedienteImportacion>();
    public DbSet<HitoExpediente> HitosExpediente => Set<HitoExpediente>();
    public DbSet<RecepcionCompra> RecepcionesCompra => Set<RecepcionCompra>();
    public DbSet<RecepcionCompraLinea> RecepcionCompraLineas => Set<RecepcionCompraLinea>();
    public DbSet<HojaImportacion> HojasImportacion => Set<HojaImportacion>();
    public DbSet<GastoImportacion> GastosImportacion => Set<GastoImportacion>();
    public DbSet<ImportacionLinea> ImportacionLineas => Set<ImportacionLinea>();

    // ── ACC: Contabilidad ────────────────────────────────────────────────────
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();
    public DbSet<CierreContable> CierresContables => Set<CierreContable>();
    public DbSet<AsientoContable> AsientosContables => Set<AsientoContable>();
    public DbSet<AsientoContableLinea> AsientoContableLineas => Set<AsientoContableLinea>();
    public DbSet<PeriodoContable> PeriodosContables => Set<PeriodoContable>();
    public DbSet<PlantillaContable> PlantillasContables => Set<PlantillaContable>();
    public DbSet<PlantillaContableLinea> PlantillaContableLineas => Set<PlantillaContableLinea>();
    public DbSet<TipoComprobante> TiposComprobante => Set<TipoComprobante>();
    public DbSet<TipoCambio> TiposCambio => Set<TipoCambio>();
    public DbSet<TipoPago> TiposPago => Set<TipoPago>();
    public DbSet<PresupuestoContable> PresupuestosContables => Set<PresupuestoContable>();
    public DbSet<PresupuestoContableLinea> PresupuestosContablesLineas => Set<PresupuestoContableLinea>();

    // ── LOG: Logística ────────────────────────────────────────────────────────
    public DbSet<HojaRuta> HojasRuta => Set<HojaRuta>();
    public DbSet<HojaRutaHistorial> HojaRutaHistorial => Set<HojaRutaHistorial>();

    // ── WF: Workflow ──────────────────────────────────────────────────────────
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<PlantillaTarea> PlantillasTareas => Set<PlantillaTarea>();

    // ── TRB: Tributario ───────────────────────────────────────────────────────
    public DbSet<RegistroImpuesto> RegistrosImpuesto => Set<RegistroImpuesto>();

    // ── ACT: Activos Fijos ────────────────────────────────────────────────────
    public DbSet<ActivoFijo> ActivosFijos => Set<ActivoFijo>();
    public DbSet<DepreciacionMensual> DepreciacionesMensuales => Set<DepreciacionMensual>();

    // ── BNC: Conciliacion Bancaria ────────────────────────────────────────────
    public DbSet<ExtractoBancario> ExtractosBancarios => Set<ExtractoBancario>();
    public DbSet<ConciliacionBancaria> ConciliacionesBancarias => Set<ConciliacionBancaria>();

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

    // ── Soporte transaccional explícito ──────────────────────────────────────

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null) return;
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No hay una transacción activa para confirmar.");

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null) return;
        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <summary>
    /// Envuelve <paramref name="operation"/> en una transacción compatible con
    /// <see cref="Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy"/>.
    /// Requerido cuando SQL Server tiene SqlServerRetryingExecutionStrategy activa.
    /// </summary>
    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc cref="ExecuteInTransactionAsync(Func{Task},CancellationToken)"/>
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await tx.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}

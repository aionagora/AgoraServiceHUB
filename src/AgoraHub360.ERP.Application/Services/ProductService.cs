namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

/// <summary>Servicio para el catálogo global de productos (plantillas).</summary>
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repo;
    private readonly IRepository<Catalog> _catalogRepo;
    private readonly IRepository<Brand> _brandRepo;
    private readonly IRepository<Manufacturer> _manufacturerRepo;
    private readonly IRepository<Uom> _uomRepo;
    private readonly IRepository<ProductStatus> _statusRepo;
    private readonly IUnitOfWork _uow;

    public ProductService(
        IRepository<Product> repo,
        IRepository<Catalog> catalogRepo,
        IRepository<Brand> brandRepo,
        IRepository<Manufacturer> manufacturerRepo,
        IRepository<Uom> uomRepo,
        IRepository<ProductStatus> statusRepo,
        IUnitOfWork uow)
    {
        _repo = repo;
        _catalogRepo = catalogRepo;
        _brandRepo = brandRepo;
        _manufacturerRepo = manufacturerRepo;
        _uomRepo = uomRepo;
        _statusRepo = statusRepo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<ProductDto2>>> GetAllAsync(
        long? catalogId = null, CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(
            p => catalogId == null || p.CatalogId == catalogId, ct);

        var brands = await _brandRepo.FindAsync(_ => true, ct);
        var manufacturers = await _manufacturerRepo.FindAsync(_ => true, ct);
        var uoms = await _uomRepo.FindAsync(_ => true, ct);
        var statuses = await _statusRepo.FindAsync(_ => true, ct);
        var catalogs = await _catalogRepo.FindAsync(_ => true, ct);

        var brandMap = brands.ToDictionary(b => b.BrandId, b => b.Nombre);
        var mfgMap = manufacturers.ToDictionary(m => m.ManufacturerId, m => m.Nombre);
        var uomMap = uoms.ToDictionary(u => u.UomId, u => u.Code);
        var statusMap = statuses.ToDictionary(s => s.ProductStatusId, s => s.Code);
        var catalogMap = catalogs.ToDictionary(c => c.CatalogId, c => c.Nombre);

        return Result<IReadOnlyList<ProductDto2>>.Success(
            items.Select(p => Map(p, brandMap, mfgMap, uomMap, statusMap, catalogMap))
                 .ToList().AsReadOnly());
    }

    public async Task<Result<ProductDto2>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductDto2>.Failure($"Producto {id} no encontrado.");
        return Result<ProductDto2>.Success(await MapSingleAsync(entity, ct));
    }

    public async Task<Result<ProductDto2>> CreateAsync(CreateProductDto2 dto, CancellationToken ct = default)
    {
        var catalog = await _catalogRepo.GetByIdAsync(dto.CatalogId, ct);
        if (catalog is null) return Result<ProductDto2>.Failure("Catálogo no encontrado.");

        var uom = await _uomRepo.GetByIdAsync(dto.DefaultUomId, ct);
        if (uom is null) return Result<ProductDto2>.Failure("Unidad de medida no encontrada.");

        int statusId = dto.LifecycleStatusId;
        if (statusId == 0)
        {
            var defStatus = (await _statusRepo.FindAsync(s => s.IsDefault, ct)).FirstOrDefault();
            if (defStatus is null) return Result<ProductDto2>.Failure("No hay estado de producto predeterminado.");
            statusId = defStatus.ProductStatusId;
        }

        var entity = new Product
        {
            CatalogId = dto.CatalogId,
            ProductKind = dto.ProductKind,
            NombreGenerico = dto.NombreGenerico,
            NombreComercial = dto.NombreComercial,
            DescripcionCorta = dto.DescripcionCorta,
            DescripcionLarga = dto.DescripcionLarga,
            BrandId = dto.BrandId,
            ManufacturerId = dto.ManufacturerId,
            DefaultUomId = dto.DefaultUomId,
            IsStockable = dto.IsStockable,
            IsSellable = dto.IsSellable,
            IsPurchasable = dto.IsPurchasable,
            LifecycleStatusId = statusId,
            Activo = true
        };

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ProductDto2>.Success(await MapSingleAsync(entity, ct));
    }

    public async Task<Result<ProductDto2>> UpdateAsync(long id, UpdateProductDto2 dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductDto2>.Failure($"Producto {id} no encontrado.");

        entity.ProductKind = dto.ProductKind;
        entity.NombreGenerico = dto.NombreGenerico;
        entity.NombreComercial = dto.NombreComercial;
        entity.DescripcionCorta = dto.DescripcionCorta;
        entity.DescripcionLarga = dto.DescripcionLarga;
        entity.BrandId = dto.BrandId;
        entity.ManufacturerId = dto.ManufacturerId;
        entity.DefaultUomId = dto.DefaultUomId;
        entity.IsStockable = dto.IsStockable;
        entity.IsSellable = dto.IsSellable;
        entity.IsPurchasable = dto.IsPurchasable;
        entity.LifecycleStatusId = dto.LifecycleStatusId;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ProductDto2>.Success(await MapSingleAsync(entity, ct));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Producto {id} no encontrado.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private async Task<ProductDto2> MapSingleAsync(Product p, CancellationToken ct)
    {
        var brandName = p.BrandId.HasValue
            ? (await _brandRepo.GetByIdAsync(p.BrandId.Value, ct))?.Nombre
            : null;
        var mfgName = p.ManufacturerId.HasValue
            ? (await _manufacturerRepo.GetByIdAsync(p.ManufacturerId.Value, ct))?.Nombre
            : null;
        var uomCode = (await _uomRepo.GetByIdAsync(p.DefaultUomId, ct))?.Code ?? "";
        var statusCode = (await _statusRepo.GetByIdAsync(p.LifecycleStatusId, ct))?.Code ?? "";
        var catalogName = (await _catalogRepo.GetByIdAsync(p.CatalogId, ct))?.Nombre ?? "";

        return Map(p,
            p.BrandId.HasValue ? new() { { p.BrandId.Value, brandName ?? "" } } : new(),
            p.ManufacturerId.HasValue ? new() { { p.ManufacturerId.Value, mfgName ?? "" } } : new(),
            new() { { p.DefaultUomId, uomCode } },
            new() { { p.LifecycleStatusId, statusCode } },
            new() { { p.CatalogId, catalogName } });
    }

    private static ProductDto2 Map(
        Product p,
        Dictionary<long, string> brandMap,
        Dictionary<long, string> mfgMap,
        Dictionary<int, string> uomMap,
        Dictionary<int, string> statusMap,
        Dictionary<long, string> catalogMap) => new(
            p.ProductId,
            p.CatalogId,
            catalogMap.TryGetValue(p.CatalogId, out var cn) ? cn : "",
            p.ProductKind,
            p.NombreGenerico,
            p.NombreComercial,
            p.DescripcionCorta,
            p.DescripcionLarga,
            p.BrandId,
            p.BrandId.HasValue && brandMap.TryGetValue(p.BrandId.Value, out var bn) ? bn : null,
            p.ManufacturerId,
            p.ManufacturerId.HasValue && mfgMap.TryGetValue(p.ManufacturerId.Value, out var mn) ? mn : null,
            p.DefaultUomId,
            uomMap.TryGetValue(p.DefaultUomId, out var uc) ? uc : "",
            p.IsStockable,
            p.IsSellable,
            p.IsPurchasable,
            p.LifecycleStatusId,
            statusMap.TryGetValue(p.LifecycleStatusId, out var sc) ? sc : "",
            p.Activo);
}

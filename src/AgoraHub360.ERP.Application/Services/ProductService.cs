namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

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
        _repo = repo; _catalogRepo = catalogRepo; _brandRepo = brandRepo;
        _manufacturerRepo = manufacturerRepo; _uomRepo = uomRepo;
        _statusRepo = statusRepo; _uow = uow;
    }

    public async Task<Result<IReadOnlyList<ProductDto2>>> GetAllAsync(
        long? catalogId = null, CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(
            p => catalogId == null || p.CatalogId == catalogId, ct);
        return Result<IReadOnlyList<ProductDto2>>.Success(
            (await BuildMapsAsync(ct, items, fullMaps: true)).AsReadOnly());
    }

    public async Task<Result<ProductDto2>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync((int)id, ct);
        if (entity is null) return Result<ProductDto2>.Failure($"Product {id} not found.");
        return Result<ProductDto2>.Success(await MapSingleAsync(entity, ct));
    }

    public async Task<Result<ProductDto2>> CreateAsync(CreateProductDto2 dto, CancellationToken ct = default)
    {
        var catalog = await _catalogRepo.GetByIdAsync((int)dto.CatalogId, ct);
        if (catalog is null) return Result<ProductDto2>.Failure("Catalog not found.");

        var uom = await _uomRepo.GetByIdAsync(dto.DefaultUomId, ct);
        if (uom is null) return Result<ProductDto2>.Failure("Unit of measure not found.");

        int statusId = dto.LifecycleStatusId;
        if (statusId == 0)
        {
            var defStatus = (await _statusRepo.FindAsync(s => s.IsDefault, ct)).FirstOrDefault();
            if (defStatus is null) return Result<ProductDto2>.Failure("No default product status configured.");
            statusId = defStatus.ProductStatusId;
        }

        var entity = new Product
        {
            CatalogId = dto.CatalogId,
            ProductKind = dto.ProductKind,
            GenericName = dto.GenericName,
            CommercialName = dto.CommercialName,
            ShortDescription = dto.ShortDescription,
            LongDescription = dto.LongDescription,
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
        var entity = await _repo.GetByIdAsync((int)id, ct);
        if (entity is null) return Result<ProductDto2>.Failure($"Product {id} not found.");

        entity.ProductKind = dto.ProductKind;
        entity.GenericName = dto.GenericName;
        entity.CommercialName = dto.CommercialName;
        entity.ShortDescription = dto.ShortDescription;
        entity.LongDescription = dto.LongDescription;
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
        var entity = await _repo.GetByIdAsync((int)id, ct);
        if (entity is null) return Result<bool>.Failure($"Product {id} not found.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<ProductDto2> MapSingleAsync(Product p, CancellationToken ct)
    {
        var brandName = p.BrandId.HasValue
            ? (await _brandRepo.GetByIdAsync((int)p.BrandId.Value, ct))?.Name : null;
        var mfgName = p.ManufacturerId.HasValue
            ? (await _manufacturerRepo.GetByIdAsync((int)p.ManufacturerId.Value, ct))?.Name : null;
        var uomCode = (await _uomRepo.GetByIdAsync(p.DefaultUomId, ct))?.Code ?? "";
        var statusCode = (await _statusRepo.GetByIdAsync(p.LifecycleStatusId, ct))?.Code ?? "";
        var catalogName = (await _catalogRepo.GetByIdAsync((int)p.CatalogId, ct))?.Name ?? "";
        return Map(p, catalogName, brandName, mfgName, uomCode, statusCode);
    }

    private async Task<List<ProductDto2>> BuildMapsAsync(
        CancellationToken ct, IReadOnlyList<Product> items, bool fullMaps)
    {
        var brands = await _brandRepo.FindAsync(_ => true, ct);
        var mfgs = await _manufacturerRepo.FindAsync(_ => true, ct);
        var uoms = await _uomRepo.FindAsync(_ => true, ct);
        var statuses = await _statusRepo.FindAsync(_ => true, ct);
        var catalogs = await _catalogRepo.FindAsync(_ => true, ct);

        var bMap = brands.ToDictionary(b => b.BrandId, b => b.Name);
        var mMap = mfgs.ToDictionary(m => m.ManufacturerId, m => m.Name);
        var uMap = uoms.ToDictionary(u => u.UomId, u => u.Code);
        var sMap = statuses.ToDictionary(s => s.ProductStatusId, s => s.Code);
        var cMap = catalogs.ToDictionary(c => c.CatalogId, c => c.Name);

        return items.Select(p => Map(p,
            cMap.TryGetValue(p.CatalogId, out var cn) ? cn : "",
            p.BrandId.HasValue && bMap.TryGetValue(p.BrandId.Value, out var bn) ? bn : null,
            p.ManufacturerId.HasValue && mMap.TryGetValue(p.ManufacturerId.Value, out var mn) ? mn : null,
            uMap.TryGetValue(p.DefaultUomId, out var uc) ? uc : "",
            sMap.TryGetValue(p.LifecycleStatusId, out var sc) ? sc : "")).ToList();
    }

    private static ProductDto2 Map(Product p,
        string catalogName, string? brandName, string? mfgName,
        string uomCode, string statusCode) => new ProductDto2(
            p.ProductId, p.CatalogId, catalogName,
            p.ProductKind, p.GenericName, p.CommercialName,
            p.ShortDescription, p.LongDescription,
            p.BrandId, brandName, p.ManufacturerId, mfgName,
            p.DefaultUomId, uomCode,
            p.IsStockable, p.IsSellable, p.IsPurchasable,
            p.LifecycleStatusId, statusCode, p.Activo);
}

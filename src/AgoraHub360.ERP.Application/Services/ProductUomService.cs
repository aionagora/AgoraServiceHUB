namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProductUomService : IProductUomService
{
    private readonly IRepository<ProductUom> _repo;
    private readonly IRepository<Uom> _uomRepo;
    private readonly IUnitOfWork _uow;

    public ProductUomService(
        IRepository<ProductUom> repo,
        IRepository<Uom> uomRepo,
        IUnitOfWork uow)
    {
        _repo = repo;
        _uomRepo = uomRepo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<ProductUomDto>>> GetByProductAsync(long productId, CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(pu => pu.ProductId == productId, ct);
        var uomIds = items.Select(pu => pu.UomId).Distinct().ToList();
        var uoms = await _uomRepo.FindAsync(u => uomIds.Contains(u.UomId), ct);
        var uomMap = uoms.ToDictionary(u => u.UomId, u => (u.Code, u.Name));

        return Result<IReadOnlyList<ProductUomDto>>.Success(
            items.Select(pu => Map(pu, uomMap)).ToList().AsReadOnly());
    }

    public async Task<Result<ProductUomDto>> CreateAsync(CreateProductUomDto dto, CancellationToken ct = default)
    {
        var dup = await _repo.FindAsync(
            pu => pu.ProductId == dto.ProductId && pu.UomId == dto.UomId, ct);
        if (dup.Any())
            return Result<ProductUomDto>.Failure("Esta unidad de medida ya está asignada al producto.");

        var uom = await _uomRepo.GetByIdAsync(dto.UomId, ct);
        if (uom is null) return Result<ProductUomDto>.Failure($"UdM {dto.UomId} no encontrada.");

        if (dto.IsBase)
        {
            var existingBase = await _repo.FindAsync(
                pu => pu.ProductId == dto.ProductId && pu.IsBase, ct);
            foreach (var eb in existingBase)
            {
                eb.IsBase = false;
                await _repo.UpdateAsync(eb, ct);
            }
        }

        var entity = new ProductUom
        {
            ProductId = dto.ProductId,
            UomId = dto.UomId,
            IsBase = dto.IsBase,
            FactorToBase = dto.FactorToBase,
            Barcode = dto.Barcode,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        var uomMap = new Dictionary<int, (string Code, string Name)>
        {
            { uom.UomId, (uom.Code, uom.Name) }
        };
        return Result<ProductUomDto>.Success(Map(entity, uomMap));
    }

    public async Task<Result<ProductUomDto>> UpdateAsync(long id, UpdateProductUomDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductUomDto>.Failure($"ProductUom {id} no encontrado.");

        if (dto.IsBase && !entity.IsBase)
        {
            var existingBase = await _repo.FindAsync(
                pu => pu.ProductId == entity.ProductId && pu.IsBase && pu.ProductUomId != id, ct);
            foreach (var eb in existingBase)
            {
                eb.IsBase = false;
                await _repo.UpdateAsync(eb, ct);
            }
        }

        entity.IsBase = dto.IsBase;
        entity.FactorToBase = dto.FactorToBase;
        entity.Barcode = dto.Barcode;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        var uom = await _uomRepo.GetByIdAsync(entity.UomId, ct);
        var uomMap = new Dictionary<int, (string Code, string Name)>
        {
            { entity.UomId, (uom?.Code ?? "", uom?.Name ?? "") }
        };
        return Result<ProductUomDto>.Success(Map(entity, uomMap));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"ProductUom {id} no encontrado.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ProductUomDto Map(ProductUom pu, Dictionary<int, (string Code, string Name)> uomMap)
    {
        var (code, name) = uomMap.TryGetValue(pu.UomId, out var u) ? u : ("", "");
        return new(pu.ProductUomId, pu.ProductId, pu.UomId, code, name,
            pu.IsBase, pu.FactorToBase, pu.Barcode, pu.Activo);
    }
}

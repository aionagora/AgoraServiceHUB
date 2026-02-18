namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProductVariantService : IProductVariantService
{
    private readonly IRepository<ProductVariant> _repo;
    private readonly IRepository<VariantAttributeValue> _axisRepo;
    private readonly IRepository<AttributeDefinition> _attrRepo;
    private readonly IRepository<AttributeOption> _optRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ProductVariantService(
        IRepository<ProductVariant> repo,
        IRepository<VariantAttributeValue> axisRepo,
        IRepository<AttributeDefinition> attrRepo,
        IRepository<AttributeOption> optRepo,
        IRepository<Product> productRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _axisRepo = axisRepo;
        _attrRepo = attrRepo;
        _optRepo = optRepo;
        _productRepo = productRepo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ProductVariantDto>>> GetByProductAsync(
        long parentProductId, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        var variants = await _repo.FindAsync(
            v => v.ParentProductId == parentProductId &&
                 (!empresaId.HasValue || v.EmpresaId == empresaId.Value), ct);

        var result = new List<ProductVariantDto>();
        var product = await _productRepo.GetByIdAsync(parentProductId, ct);
        foreach (var v in variants)
            result.Add(await MapAsync(v, product?.NombreComercial ?? "", ct));

        return Result<IReadOnlyList<ProductVariantDto>>.Success(result.AsReadOnly());
    }

    public async Task<Result<ProductVariantDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductVariantDto>.Failure($"Variante {id} no encontrada.");
        var product = await _productRepo.GetByIdAsync(entity.ParentProductId, ct);
        return Result<ProductVariantDto>.Success(await MapAsync(entity, product?.NombreComercial ?? "", ct));
    }

    public async Task<Result<ProductVariantDto>> CreateAsync(
        CreateProductVariantDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue) return Result<ProductVariantDto>.Failure("Sin empresa activa.");

        var dup = await _repo.FindAsync(
            v => v.EmpresaId == empresaId.Value && v.Sku == dto.Sku, ct);
        if (dup.Any()) return Result<ProductVariantDto>.Failure($"El SKU '{dto.Sku}' ya existe para esta empresa.");

        var entity = new ProductVariant
        {
            EmpresaId = empresaId.Value,
            ParentProductId = dto.ParentProductId,
            Sku = dto.Sku,
            Barcode = dto.Barcode,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        if (dto.AxisValues is not null)
            await SaveAxisValuesAsync(entity.VariantId, dto.AxisValues, ct);

        var product = await _productRepo.GetByIdAsync(dto.ParentProductId, ct);
        return Result<ProductVariantDto>.Success(await MapAsync(entity, product?.NombreComercial ?? "", ct));
    }

    public async Task<Result<ProductVariantDto>> UpdateAsync(
        long id, UpdateProductVariantDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductVariantDto>.Failure($"Variante {id} no encontrada.");
        if (_currentUser.EmpresaId.HasValue && entity.EmpresaId != _currentUser.EmpresaId.Value)
            return Result<ProductVariantDto>.Failure("Sin acceso.");

        var dup = await _repo.FindAsync(
            v => v.EmpresaId == entity.EmpresaId && v.Sku == dto.Sku && v.VariantId != id, ct);
        if (dup.Any()) return Result<ProductVariantDto>.Failure($"El SKU '{dto.Sku}' ya existe.");

        entity.Sku = dto.Sku;
        entity.Barcode = dto.Barcode;
        entity.VariantName = dto.VariantName;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        if (dto.AxisValues is not null)
            await SaveAxisValuesAsync(id, dto.AxisValues, ct);

        await _uow.SaveChangesAsync(ct);
        var product = await _productRepo.GetByIdAsync(entity.ParentProductId, ct);
        return Result<ProductVariantDto>.Success(await MapAsync(entity, product?.NombreComercial ?? "", ct));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Variante {id} no encontrada.");
        if (_currentUser.EmpresaId.HasValue && entity.EmpresaId != _currentUser.EmpresaId.Value)
            return Result<bool>.Failure("Sin acceso.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private async Task SaveAxisValuesAsync(
        long variantId, IList<VariantAxisValueDto> axisValues, CancellationToken ct)
    {
        var existing = await _axisRepo.FindAsync(v => v.VariantId == variantId, ct);
        foreach (var e in existing) await _axisRepo.DeleteAsync(e, ct);

        foreach (var av in axisValues)
        {
            await _axisRepo.AddAsync(new VariantAttributeValue
            {
                VariantId = variantId,
                AttributeId = av.AttributeId,
                OptionId = av.OptionId,
                ValueString = av.ValueString ?? av.OptionValue
            }, ct);
        }
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<ProductVariantDto> MapAsync(ProductVariant v, string parentName, CancellationToken ct)
    {
        var axisValues = await _axisRepo.FindAsync(av => av.VariantId == v.VariantId, ct);
        var attrIds = axisValues.Select(av => av.AttributeId).ToList();
        var attrs = await _attrRepo.FindAsync(a => attrIds.Contains(a.AttributeId), ct);
        var attrMap = attrs.ToDictionary(a => a.AttributeId, a => a.Name);

        var optIds = axisValues.Where(av => av.OptionId.HasValue).Select(av => av.OptionId!.Value).ToList();
        var opts = await _optRepo.FindAsync(o => optIds.Contains(o.OptionId), ct);
        var optMap = opts.ToDictionary(o => o.OptionId, o => o.Value);

        var axisDtos = axisValues.Select(av => new VariantAxisValueDto(
            av.AttributeId,
            attrMap.TryGetValue(av.AttributeId, out var an) ? an : "",
            av.OptionId,
            av.OptionId.HasValue && optMap.TryGetValue(av.OptionId.Value, out var ov) ? ov : null,
            av.ValueString)).ToList().AsReadOnly();

        return new ProductVariantDto(
            v.VariantId, v.EmpresaId, v.ParentProductId, parentName,
            v.Sku, v.Barcode, v.VariantName, v.Activo, axisDtos);
    }
}

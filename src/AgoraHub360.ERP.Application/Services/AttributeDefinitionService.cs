namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class AttributeDefinitionService : IAttributeDefinitionService
{
    private readonly IRepository<AttributeDefinition> _repo;
    private readonly IRepository<AttributeOption> _optionRepo;
    private readonly IRepository<ProductAttribute> _paRepo;
    private readonly IUnitOfWork _uow;

    public AttributeDefinitionService(
        IRepository<AttributeDefinition> repo,
        IRepository<AttributeOption> optionRepo,
        IRepository<ProductAttribute> paRepo,
        IUnitOfWork uow)
    {
        _repo = repo;
        _optionRepo = optionRepo;
        _paRepo = paRepo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<AttributeDefinitionDto>>> GetByIndustryAsync(
        int industryId, CancellationToken ct = default)
    {
        var attrs = await _repo.FindAsync(a => a.IndustryId == industryId, ct);
        var options = await _optionRepo.FindAsync(_ => true, ct);
        var optMap = options.GroupBy(o => o.AttributeId)
                           .ToDictionary(g => g.Key, g => g.ToList());

        return Result<IReadOnlyList<AttributeDefinitionDto>>.Success(
            attrs.Select(a => Map(a, optMap.TryGetValue(a.AttributeId, out var opts) ? opts : new()))
                 .ToList().AsReadOnly());
    }

    public async Task<Result<AttributeDefinitionDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<AttributeDefinitionDto>.Failure($"Atributo {id} no encontrado.");
        var options = await _optionRepo.FindAsync(o => o.AttributeId == id, ct);
        return Result<AttributeDefinitionDto>.Success(Map(entity, options.ToList()));
    }

    public async Task<Result<AttributeDefinitionDto>> CreateAsync(
        CreateAttributeDefinitionDto dto, CancellationToken ct = default)
    {
        var dup = await _repo.FindAsync(a => a.IndustryId == dto.IndustryId && a.Code == dto.Code, ct);
        if (dup.Any())
            return Result<AttributeDefinitionDto>.Failure($"Ya existe el atributo '{dto.Code}' para esta industria.");

        var entity = new AttributeDefinition
        {
            IndustryId = dto.IndustryId,
            Code = dto.Code,
            Name = dto.Name,
            DataType = dto.DataType,
            IsRequired = dto.IsRequired,
            IsSearchable = dto.IsSearchable,
            IsVariantAxis = dto.IsVariantAxis,
            ValidationRegex = dto.ValidationRegex,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            UnitHint = dto.UnitHint,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<AttributeDefinitionDto>.Success(Map(entity, new()));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Atributo {id} no encontrado.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<AttributeOptionDto>> AddOptionAsync(
        CreateAttributeOptionDto dto, CancellationToken ct = default)
    {
        var attr = await _repo.GetByIdAsync(dto.AttributeId, ct);
        if (attr is null) return Result<AttributeOptionDto>.Failure("Atributo no encontrado.");

        var option = new AttributeOption
        {
            AttributeId = dto.AttributeId,
            Value = dto.Value,
            SortOrder = dto.SortOrder,
            Activo = true
        };
        await _optionRepo.AddAsync(option, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<AttributeOptionDto>.Success(MapOption(option));
    }

    public async Task<Result<AttributeOptionDto>> UpdateOptionAsync(long optionId, UpdateAttributeOptionDto dto, CancellationToken ct = default)
    {
        var option = await _optionRepo.GetByIdAsync(optionId, ct);
        if (option is null) return Result<AttributeOptionDto>.Failure($"Opción {optionId} no encontrada.");

        option.Value = dto.Value;
        option.SortOrder = dto.SortOrder;
        await _optionRepo.UpdateAsync(option, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<AttributeOptionDto>.Success(MapOption(option));
    }

    public async Task<Result<bool>> DeleteOptionAsync(long optionId, CancellationToken ct = default)
    {
        var option = await _optionRepo.GetByIdAsync(optionId, ct);
        if (option is null) return Result<bool>.Failure($"Opción {optionId} no encontrada.");
        await _optionRepo.DeleteAsync(option, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<ProductAttributeDto>>> GetProductAttributesAsync(
        long productId, CancellationToken ct = default)
    {
        var pas = await _paRepo.FindAsync(pa => pa.ProductId == productId, ct);
        var attrIds = pas.Select(pa => pa.AttributeId).Distinct().ToList();
        var attrs = await _repo.FindAsync(a => attrIds.Contains(a.AttributeId), ct);
        var attrMap = attrs.ToDictionary(a => a.AttributeId, a => a.Name);

        var optionIds = pas.Where(pa => pa.OptionId.HasValue).Select(pa => pa.OptionId!.Value).Distinct().ToList();
        var options = await _optionRepo.FindAsync(o => optionIds.Contains(o.OptionId), ct);
        var optMap = options.ToDictionary(o => o.OptionId, o => o.Value);

        return Result<IReadOnlyList<ProductAttributeDto>>.Success(
            pas.Select(pa => new ProductAttributeDto(
                pa.ProductAttributeId, pa.ProductId, pa.AttributeId,
                attrMap.TryGetValue(pa.AttributeId, out var an) ? an : "",
                pa.ValueString, pa.ValueDecimal, pa.ValueInt, pa.ValueBool,
                pa.ValueDate, pa.ValueJson, pa.OptionId,
                pa.OptionId.HasValue && optMap.TryGetValue(pa.OptionId.Value, out var ov) ? ov : null,
                pa.ValidFrom, pa.ValidTo))
            .ToList().AsReadOnly());
    }

    public async Task<Result<ProductAttributeDto>> UpsertProductAttributeAsync(
        UpsertProductAttributeDto dto, CancellationToken ct = default)
    {
        var attr = await _repo.GetByIdAsync(dto.AttributeId, ct);
        if (attr is null) return Result<ProductAttributeDto>.Failure("Atributo no encontrado.");

        var existing = (await _paRepo.FindAsync(
            pa => pa.ProductId == dto.ProductId && pa.AttributeId == dto.AttributeId
               && pa.ValidFrom == dto.ValidFrom, ct)).FirstOrDefault();

        string? optValue = null;
        if (dto.OptionId.HasValue)
        {
            var opt = await _optionRepo.GetByIdAsync(dto.OptionId.Value, ct);
            optValue = opt?.Value;
        }

        if (existing is not null)
        {
            existing.ValueString = dto.ValueString;
            existing.ValueDecimal = dto.ValueDecimal;
            existing.ValueInt = dto.ValueInt;
            existing.ValueBool = dto.ValueBool;
            existing.ValueDate = dto.ValueDate;
            existing.ValueJson = dto.ValueJson;
            existing.OptionId = dto.OptionId;
            existing.ValidTo = dto.ValidTo;
            await _paRepo.UpdateAsync(existing, ct);
            await _uow.SaveChangesAsync(ct);
            return Result<ProductAttributeDto>.Success(new ProductAttributeDto(
                existing.ProductAttributeId, existing.ProductId, existing.AttributeId, attr.Name,
                existing.ValueString, existing.ValueDecimal, existing.ValueInt, existing.ValueBool,
                existing.ValueDate, existing.ValueJson, existing.OptionId, optValue,
                existing.ValidFrom, existing.ValidTo));
        }

        var pa = new ProductAttribute
        {
            ProductId = dto.ProductId,
            AttributeId = dto.AttributeId,
            ValueString = dto.ValueString,
            ValueDecimal = dto.ValueDecimal,
            ValueInt = dto.ValueInt,
            ValueBool = dto.ValueBool,
            ValueDate = dto.ValueDate,
            ValueJson = dto.ValueJson,
            OptionId = dto.OptionId,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            Activo = true
        };
        await _paRepo.AddAsync(pa, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ProductAttributeDto>.Success(new ProductAttributeDto(
            pa.ProductAttributeId, pa.ProductId, pa.AttributeId, attr.Name,
            pa.ValueString, pa.ValueDecimal, pa.ValueInt, pa.ValueBool,
            pa.ValueDate, pa.ValueJson, pa.OptionId, optValue,
            pa.ValidFrom, pa.ValidTo));
    }

    private static AttributeDefinitionDto Map(AttributeDefinition a, List<AttributeOption> options) => new(
        a.AttributeId, a.IndustryId, "", a.Code, a.Name, a.DataType,
        a.IsRequired, a.IsSearchable, a.IsVariantAxis,
        a.ValidationRegex, a.MinValue, a.MaxValue, a.UnitHint, a.Activo,
        options.Select(MapOption).ToList().AsReadOnly());

    private static AttributeOptionDto MapOption(AttributeOption o) => new(o.OptionId, o.AttributeId, o.Value, o.SortOrder);
}

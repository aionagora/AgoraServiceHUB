namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.RUL;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.RUL;

public class IndustryService : IIndustryService
{
    private readonly IRepository<Industry> _repo;
    private readonly IRepository<ProductIndustryRule> _ruleRepo;
    private readonly IUnitOfWork _uow;

    public IndustryService(
        IRepository<Industry> repo,
        IRepository<ProductIndustryRule> ruleRepo,
        IUnitOfWork uow)
    {
        _repo = repo;
        _ruleRepo = ruleRepo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<IndustryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(_ => true, ct);
        return Result<IReadOnlyList<IndustryDto>>.Success(
            items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<IndustryDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<IndustryDto>.Failure($"Industria {id} no encontrada.");
        return Result<IndustryDto>.Success(Map(entity));
    }

    public async Task<Result<IndustryDto>> CreateAsync(CreateIndustryDto dto, CancellationToken ct = default)
    {
        var dup = await _repo.FindAsync(i => i.Code == dto.Code, ct);
        if (dup.Any()) return Result<IndustryDto>.Failure($"Ya existe la industria '{dto.Code}'.");

        var entity = new Industry { Code = dto.Code, Name = dto.Name, Activo = true };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<IndustryDto>.Success(Map(entity));
    }

    public async Task<Result<IndustryDto>> UpdateAsync(int id, UpdateIndustryDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<IndustryDto>.Failure($"Industria {id} no encontrada.");
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Activo = dto.Activo;
        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<IndustryDto>.Success(Map(entity));
    }

    public async Task<Result<IReadOnlyList<ProductIndustryRuleDto>>> GetRulesAsync(
        int industryId, CancellationToken ct = default)
    {
        var rules = await _ruleRepo.FindAsync(r => r.IndustryId == industryId, ct);
        var industry = await _repo.GetByIdAsync(industryId, ct);
        var code = industry?.Code ?? "";
        return Result<IReadOnlyList<ProductIndustryRuleDto>>.Success(
            rules.Select(r => MapRule(r, code)).ToList().AsReadOnly());
    }

    public async Task<Result<ProductIndustryRuleDto>> CreateRuleAsync(
        CreateProductIndustryRuleDto dto, CancellationToken ct = default)
    {
        var industry = await _repo.GetByIdAsync(dto.IndustryId, ct);
        if (industry is null) return Result<ProductIndustryRuleDto>.Failure("Industria no encontrada.");

        var rule = new ProductIndustryRule
        {
            IndustryId = dto.IndustryId,
            ConditionJson = dto.ConditionJson,
            ActionsJson = dto.ActionsJson,
            Priority = dto.Priority,
            Activo = true
        };
        await _ruleRepo.AddAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ProductIndustryRuleDto>.Success(MapRule(rule, industry.Code));
    }

    public async Task<Result<bool>> DeleteRuleAsync(long ruleId, CancellationToken ct = default)
    {
        var rule = await _ruleRepo.GetByIdAsync((int)ruleId, ct);
        if (rule is null) return Result<bool>.Failure($"Regla {ruleId} no encontrada.");
        await _ruleRepo.DeleteAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static IndustryDto Map(Industry i) => new(i.IndustryId, i.Code, i.Name, i.Activo);

    private static ProductIndustryRuleDto MapRule(ProductIndustryRule r, string industryCode) => new(
        r.RuleId, r.IndustryId, industryCode, r.ConditionJson, r.ActionsJson, r.Priority, r.Activo);
}

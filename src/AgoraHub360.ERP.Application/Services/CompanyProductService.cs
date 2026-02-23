namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.RUL;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using AgoraHub360.ERP.Shared.DTOs.RUL;
using System.Text.Json;

public class CompanyProductService : ICompanyProductService
{
    private readonly IRepository<CompanyProduct> _repo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<CompanyProductFeature> _featureRepo;
    private readonly IRepository<ProductIndustryRule> _ruleRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CompanyProductService(
        IRepository<CompanyProduct> repo,
        IRepository<Product> productRepo,
        IRepository<CompanyProductFeature> featureRepo,
        IRepository<ProductIndustryRule> ruleRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _productRepo = productRepo;
        _featureRepo = featureRepo;
        _ruleRepo = ruleRepo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CompanyProductDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<CompanyProductDto>>.Failure("No se pudo determinar la empresa activa.");

        var items = await _repo.FindAsync(cp => cp.EmpresaId == empresaId.Value, ct);
        var products = await _productRepo.FindAsync(_ => true, ct);
        var productMap = products.ToDictionary(p => p.ProductId, p => p.CommercialName);

        return Result<IReadOnlyList<CompanyProductDto>>.Success(
            items.Select(cp => Map(cp, productMap)).ToList().AsReadOnly());
    }

    public async Task<Result<CompanyProductDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<CompanyProductDto>.Failure($"CompanyProduct {id} no encontrado.");
        if (!CanAccess(entity)) return Result<CompanyProductDto>.Failure("Sin acceso.");

        var product = await _productRepo.GetByIdAsync(entity.ProductId, ct);
        return Result<CompanyProductDto>.Success(Map(entity,
            new Dictionary<long, string> { { entity.ProductId, product?.CommercialName ?? "" } }));
    }

    public async Task<Result<CompanyProductDto>> CreateAsync(CreateCompanyProductDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CompanyProductDto>.Failure("No se pudo determinar la empresa activa.");

        var product = await _productRepo.GetByIdAsync(dto.ProductId, ct);
        if (product is null) return Result<CompanyProductDto>.Failure("Producto global no encontrado.");

        var dup = await _repo.FindAsync(
            cp => cp.EmpresaId == empresaId.Value && cp.ProductId == dto.ProductId, ct);
        if (dup.Any())
            return Result<CompanyProductDto>.Failure("El producto ya está activado para esta empresa.");

        var dupSku = await _repo.FindAsync(
            cp => cp.EmpresaId == empresaId.Value && cp.Sku == dto.Sku, ct);
        if (dupSku.Any())
            return Result<CompanyProductDto>.Failure($"El SKU '{dto.Sku}' ya existe para esta empresa.");

        var entity = new CompanyProduct
        {
            EmpresaId = empresaId.Value,
            ProductId = dto.ProductId,
            Sku = dto.Sku,
            CodigoInterno = dto.CodigoInterno,
            IsVisiblePOS = dto.IsVisiblePOS,
            IsVisibleEcommerce = dto.IsVisibleEcommerce,
            IsVisibleB2B = dto.IsVisibleB2B,
            AllowReturns = dto.AllowReturns,
            WarrantyDays = dto.WarrantyDays,
            MinStock = dto.MinStock,
            MaxStock = dto.MaxStock,
            ReorderPoint = dto.ReorderPoint,
            CostingMethod = dto.CostingMethod,
            Activo = true
        };

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<CompanyProductDto>.Success(Map(entity,
            new Dictionary<long, string> { { entity.ProductId, product.CommercialName } }));
    }

    public async Task<Result<CompanyProductDto>> UpdateAsync(long id, UpdateCompanyProductDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<CompanyProductDto>.Failure($"CompanyProduct {id} no encontrado.");
        if (!CanAccess(entity)) return Result<CompanyProductDto>.Failure("Sin acceso.");

        // Verificar SKU único (excluyendo el actual)
        var dupSku = await _repo.FindAsync(
            cp => cp.EmpresaId == entity.EmpresaId && cp.Sku == dto.Sku && cp.CompanyProductId != id, ct);
        if (dupSku.Any())
            return Result<CompanyProductDto>.Failure($"El SKU '{dto.Sku}' ya existe para esta empresa.");

        entity.Sku = dto.Sku;
        entity.CodigoInterno = dto.CodigoInterno;
        entity.IsVisiblePOS = dto.IsVisiblePOS;
        entity.IsVisibleEcommerce = dto.IsVisibleEcommerce;
        entity.IsVisibleB2B = dto.IsVisibleB2B;
        entity.AllowReturns = dto.AllowReturns;
        entity.WarrantyDays = dto.WarrantyDays;
        entity.MinStock = dto.MinStock;
        entity.MaxStock = dto.MaxStock;
        entity.ReorderPoint = dto.ReorderPoint;
        entity.CostingMethod = dto.CostingMethod;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        var product = await _productRepo.GetByIdAsync(entity.ProductId, ct);
        return Result<CompanyProductDto>.Success(Map(entity,
            new Dictionary<long, string> { { entity.ProductId, product?.CommercialName ?? "" } }));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"CompanyProduct {id} no encontrado.");
        if (!CanAccess(entity)) return Result<bool>.Failure("Sin acceso.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<CompanyProductFeatureDto>>> GetFeaturesAsync(
        long companyProductId, CancellationToken ct = default)
    {
        var cp = await _repo.GetByIdAsync(companyProductId, ct);
        if (cp is null || !CanAccess(cp)) return Result<IReadOnlyList<CompanyProductFeatureDto>>.Failure("Sin acceso.");

        var features = await _featureRepo.FindAsync(f => f.CompanyProductId == companyProductId, ct);
        return Result<IReadOnlyList<CompanyProductFeatureDto>>.Success(
            features.Select(f => new CompanyProductFeatureDto(f.EmpresaId, f.CompanyProductId, f.FeatureCode, f.IsEnabled))
                    .ToList().AsReadOnly());
    }

    public async Task<Result<CompanyProductFeatureDto>> SetFeatureAsync(
        long companyProductId, SetFeatureDto dto, CancellationToken ct = default)
    {
        var cp = await _repo.GetByIdAsync(companyProductId, ct);
        if (cp is null || !CanAccess(cp)) return Result<CompanyProductFeatureDto>.Failure("Sin acceso.");

        var existing = (await _featureRepo.FindAsync(
            f => f.CompanyProductId == companyProductId && f.FeatureCode == dto.FeatureCode, ct)).FirstOrDefault();

        if (existing is not null)
        {
            existing.IsEnabled = dto.IsEnabled;
            await _featureRepo.UpdateAsync(existing, ct);
        }
        else
        {
            var feature = new CompanyProductFeature
            {
                EmpresaId = cp.EmpresaId,
                CompanyProductId = companyProductId,
                FeatureCode = dto.FeatureCode,
                IsEnabled = dto.IsEnabled
            };
            await _featureRepo.AddAsync(feature, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<CompanyProductFeatureDto>.Success(
            new CompanyProductFeatureDto(cp.EmpresaId, companyProductId, dto.FeatureCode, dto.IsEnabled));
    }

    public async Task<Result<bool>> ApplyIndustryRulesAsync(
        long companyProductId, int industryId, CancellationToken ct = default)
    {
        var cp = await _repo.GetByIdAsync(companyProductId, ct);
        if (cp is null || !CanAccess(cp)) return Result<bool>.Failure("Sin acceso.");

        var product = await _productRepo.GetByIdAsync(cp.ProductId, ct);
        if (product is null) return Result<bool>.Failure("Producto base no encontrado.");

        var rules = (await _ruleRepo.FindAsync(r => r.IndustryId == industryId && r.Activo, ct))
            .OrderBy(r => r.Priority);

        foreach (var rule in rules)
        {
            if (!EvaluateCondition(rule.ConditionJson, product, cp))
                continue;

            var actions = ParseActions(rule.ActionsJson);
            foreach (var (featureCode, isEnabled) in actions)
            {
                await SetFeatureAsync(companyProductId, new SetFeatureDto(featureCode, isEnabled), ct);
            }
        }

        return Result<bool>.Success(true);
    }

    // ──── Helpers ─────────────────────────────────────────────────────────────

    private bool CanAccess(CompanyProduct cp) =>
        !_currentUser.EmpresaId.HasValue || cp.EmpresaId == _currentUser.EmpresaId.Value;

    private static bool EvaluateCondition(string conditionJson, Product product, CompanyProduct cp)
    {
        try
        {
            var conditions = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(conditionJson);
            if (conditions is null) return true;

            foreach (var (key, value) in conditions)
            {
                var match = key switch
                {
                    "ProductKind" => product.ProductKind == value.GetByte(),
                    "IsStockable" => product.IsStockable == value.GetBoolean(),
                    "IsSellable" => product.IsSellable == value.GetBoolean(),
                    _ => true
                };
                if (!match) return false;
            }
            return true;
        }
        catch
        {
            return true;
        }
    }

    private static IEnumerable<(string FeatureCode, bool IsEnabled)> ParseActions(string actionsJson)
    {
        Dictionary<string, JsonElement>? actions = null;
        try
        {
            actions = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(actionsJson);
        }
        catch
        {
            yield break;
        }

        if (actions is null) yield break;

        foreach (var (key, value) in actions)
        {
            if (key.StartsWith("Enable") && value.ValueKind == JsonValueKind.True)
            {
                var featureCode = key.Replace("Enable", "").ToUpperInvariant();
                yield return (featureCode, true);
            }
            else if (key == "Policy" && value.ValueKind == JsonValueKind.String)
            {
                yield return (value.GetString()!, true);
            }
        }
    }

    private static CompanyProductDto Map(CompanyProduct cp, Dictionary<long, string> productMap) => new(
        cp.CompanyProductId, cp.EmpresaId, cp.ProductId,
        productMap.TryGetValue(cp.ProductId, out var pn) ? pn : "",
        cp.Sku, cp.CodigoInterno,
        cp.IsVisiblePOS, cp.IsVisibleEcommerce, cp.IsVisibleB2B,
        cp.AllowReturns, cp.WarrantyDays,
        cp.MinStock, cp.MaxStock, cp.ReorderPoint,
        cp.CostingMethod, cp.Activo);
}

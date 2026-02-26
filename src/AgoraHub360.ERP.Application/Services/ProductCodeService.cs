namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProductCodeService : IProductCodeService
{
    private readonly IRepository<ProductCode> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ProductCodeService(IRepository<ProductCode> repo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ProductCodeDto>>> GetByProductAsync(long productId, CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(c => c.ProductId == productId, ct);
        return Result<IReadOnlyList<ProductCodeDto>>.Success(items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<ProductCodeDto>> CreateAsync(CreateProductCodeDto dto, CancellationToken ct = default)
    {
        var entity = new ProductCode
        {
            EmpresaId = dto.EmpresaId,
            ProductId = dto.ProductId,
            CodeType = dto.CodeType,
            Valor = dto.Valor,
            ProviderId = dto.ProviderId,
            CustomerId = dto.CustomerId,
            ChannelId = dto.ChannelId,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsPrimary = dto.IsPrimary,
            IsActive = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ProductCodeDto>.Success(Map(entity));
    }

    public async Task<Result<ProductCodeDto>> UpdateAsync(long id, UpdateProductCodeDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductCodeDto>.Failure($"Código {id} no encontrado.");

        entity.CodeType = dto.CodeType;
        entity.Valor = dto.Valor;
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidTo = dto.ValidTo;
        entity.IsPrimary = dto.IsPrimary;
        entity.IsActive = dto.IsActive;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ProductCodeDto>.Success(Map(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Código {id} no encontrado.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ProductCodeDto Map(ProductCode c) => new(
        c.ProductCodeId, c.EmpresaId, c.ProductId, c.CodeType, c.Valor,
        c.ProviderId, c.CustomerId, c.ChannelId, c.ValidFrom, c.ValidTo,
        c.IsPrimary, c.IsActive);
}

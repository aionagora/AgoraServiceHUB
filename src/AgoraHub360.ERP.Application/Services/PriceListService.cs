namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.PRC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.PRC;

public class PriceListService : IPriceListService
{
    private readonly IRepository<PriceList> _repo;
    private readonly IRepository<PriceListItem> _itemRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public PriceListService(
        IRepository<PriceList> repo,
        IRepository<PriceListItem> itemRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _itemRepo = itemRepo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<PriceListDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue) return Result<IReadOnlyList<PriceListDto>>.Failure("Sin empresa activa.");

        var items = await _repo.FindAsync(pl => pl.EmpresaId == empresaId.Value, ct);
        return Result<IReadOnlyList<PriceListDto>>.Success(items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<PriceListDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<PriceListDto>.Failure($"Lista de precios {id} no encontrada.");
        if (!CanAccess(entity)) return Result<PriceListDto>.Failure("Sin acceso.");
        return Result<PriceListDto>.Success(Map(entity));
    }

    public async Task<Result<PriceListDto>> CreateAsync(CreatePriceListDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue) return Result<PriceListDto>.Failure("Sin empresa activa.");

        var dup = await _repo.FindAsync(pl => pl.EmpresaId == empresaId.Value && pl.Code == dto.Code, ct);
        if (dup.Any()) return Result<PriceListDto>.Failure($"Ya existe lista de precios con código '{dto.Code}'.");

        var entity = new PriceList
        {
            EmpresaId = empresaId.Value,
            Code = dto.Code,
            Name = dto.Name,
            CurrencyId = dto.CurrencyId,
            ChannelId = dto.ChannelId,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsDefault = dto.IsDefault,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<PriceListDto>.Success(Map(entity));
    }

    public async Task<Result<PriceListDto>> UpdateAsync(long id, UpdatePriceListDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<PriceListDto>.Failure($"Lista de precios {id} no encontrada.");
        if (!CanAccess(entity)) return Result<PriceListDto>.Failure("Sin acceso.");

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.CurrencyId = dto.CurrencyId;
        entity.ChannelId = dto.ChannelId;
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidTo = dto.ValidTo;
        entity.IsDefault = dto.IsDefault;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<PriceListDto>. Success(Map(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Lista de precios {id} no encontrada.");
        if (!CanAccess(entity)) return Result<bool>.Failure("Sin acceso.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<PriceListItemDto>>> GetItemsAsync(
        long priceListId, CancellationToken ct = default)
    {
        var pl = await _repo.GetByIdAsync(priceListId, ct);
        if (pl is null || !CanAccess(pl)) return Result<IReadOnlyList<PriceListItemDto>>.Failure("Sin acceso.");

        var items = await _itemRepo.FindAsync(i => i.PriceListId == priceListId, ct);
        return Result<IReadOnlyList<PriceListItemDto>>.Success(items.Select(MapItem).ToList().AsReadOnly());
    }

    public async Task<Result<PriceListItemDto>> AddItemAsync(CreatePriceListItemDto dto, CancellationToken ct = default)
    {
        var pl = await _repo.GetByIdAsync(dto.PriceListId, ct);
        if (pl is null || !CanAccess(pl)) return Result<PriceListItemDto>.Failure("Lista de precios no encontrada o sin acceso.");

        var item = new PriceListItem
        {
            PriceListId = dto.PriceListId,
            CompanyProductId = dto.CompanyProductId,
            VariantId = dto.VariantId,
            Price = dto.Price,
            MinQty = dto.MinQty,
            DiscountPercent = dto.DiscountPercent,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            Activo = true
        };
        await _itemRepo.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<PriceListItemDto>.Success(MapItem(item));
    }

    public async Task<Result<bool>> DeleteItemAsync(long itemId, CancellationToken ct = default)
    {
        var item = await _itemRepo.GetByIdAsync(itemId, ct);
        if (item is null) return Result<bool>.Failure($"Item {itemId} no encontrado.");
        await _itemRepo.DeleteAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private bool CanAccess(PriceList pl) =>
        !_currentUser.EmpresaId.HasValue || pl.EmpresaId == _currentUser.EmpresaId.Value;

    private static PriceListDto Map(PriceList pl) => new(
        pl.PriceListId, pl.EmpresaId, pl.Code, pl.Name, pl.CurrencyId,
        pl.ChannelId, pl.ValidFrom, pl.ValidTo, pl.IsDefault, pl.Activo);

    private static PriceListItemDto MapItem(PriceListItem i) => new(
        i.ItemId, i.PriceListId, i.CompanyProductId, i.VariantId,
        i.Price, i.MinQty, i.DiscountPercent, i.ValidFrom, i.ValidTo);
}

namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.UnidadMedida;

public class UnidadMedidaService : IUnidadMedidaService
{
    private readonly IRepository<UnidadMedida> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UnidadMedidaService(
        IRepository<UnidadMedida> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<UnidadMedidaDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<UnidadMedidaDto>>.Failure("No se pudo determinar la empresa activa del usuario.");

        var items = await _repository.FindAsync(u => u.EmpresaId == empresaId.Value, ct);
        return Result<IReadOnlyList<UnidadMedidaDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<UnidadMedidaDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<UnidadMedidaDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<UnidadMedidaDto>.Failure("Unidad de medida no encontrada.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<UnidadMedidaDto>.Failure("No tiene permisos para acceder a esta unidad de medida.");

        return Result<UnidadMedidaDto>.Success(MapToDto(entity));
    }

    public async Task<Result<UnidadMedidaDto>> CreateAsync(CreateUnidadMedidaDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<UnidadMedidaDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        // Validar unicidad del nombre en la empresa
        var existing = await _repository.FindAsync(
            u => u.EmpresaId == empresaId.Value && u.Nombre == dto.Nombre, ct);
        if (existing.Any())
            return Result<UnidadMedidaDto>.Failure($"Ya existe una unidad de medida con el nombre '{dto.Nombre}'.");

        // Validar unicidad de la abreviatura en la empresa
        var existingAbrev = await _repository.FindAsync(
            u => u.EmpresaId == empresaId.Value && u.Abreviatura == dto.Abreviatura, ct);
        if (existingAbrev.Any())
            return Result<UnidadMedidaDto>.Failure($"Ya existe una unidad de medida con la abreviatura '{dto.Abreviatura}'.");

        var entity = new UnidadMedida
        {
            Nombre = dto.Nombre,
            Abreviatura = dto.Abreviatura,
            EmpresaId = empresaId.Value,
            Activo = true
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<UnidadMedidaDto>.Success(MapToDto(entity));
    }

    public async Task<Result<UnidadMedidaDto>> UpdateAsync(int id, UpdateUnidadMedidaDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<UnidadMedidaDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<UnidadMedidaDto>.Failure("Unidad de medida no encontrada.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<UnidadMedidaDto>.Failure("No tiene permisos para modificar esta unidad de medida.");

        // Validar unicidad del nombre (excepto el mismo registro)
        var existing = await _repository.FindAsync(
            u => u.EmpresaId == empresaId.Value && u.Nombre == dto.Nombre && u.Id != id, ct);
        if (existing.Any())
            return Result<UnidadMedidaDto>.Failure($"Ya existe otra unidad de medida con el nombre '{dto.Nombre}'.");

        // Validar unicidad de la abreviatura (excepto el mismo registro)
        var existingAbrev = await _repository.FindAsync(
            u => u.EmpresaId == empresaId.Value && u.Abreviatura == dto.Abreviatura && u.Id != id, ct);
        if (existingAbrev.Any())
            return Result<UnidadMedidaDto>.Failure($"Ya existe otra unidad de medida con la abreviatura '{dto.Abreviatura}'.");

        entity.Nombre = dto.Nombre;
        entity.Abreviatura = dto.Abreviatura;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<UnidadMedidaDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure("Unidad de medida no encontrada.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("No tiene permisos para eliminar esta unidad de medida.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private static UnidadMedidaDto MapToDto(UnidadMedida entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Abreviatura = entity.Abreviatura,
        Activo = entity.Activo,
        EmpresaId = entity.EmpresaId
    };
}

namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class CategoriaProductoService : ICategoriaProductoService
{
    private readonly IRepository<CategoriaProducto> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CategoriaProductoService(
        IRepository<CategoriaProducto> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CategoriaProductoDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<CategoriaProductoDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<CategoriaProductoDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<CategoriaProductoDto>.Failure($"Categoría con Id {id} no encontrada.");
        return Result<CategoriaProductoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<CategoriaProductoDto>> CreateAsync(CreateCategoriaProductoDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CategoriaProductoDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar nombre único por empresa
        var existing = await _repository.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.Nombre == dto.Nombre, ct);
        if (existing.Count > 0)
            return Result<CategoriaProductoDto>.Failure($"Ya existe una categoría con nombre '{dto.Nombre}'.");

        var entity = new CategoriaProducto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            EmpresaId = empresaId.Value
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CategoriaProductoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<CategoriaProductoDto>> UpdateAsync(int id, UpdateCategoriaProductoDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<CategoriaProductoDto>.Failure($"Categoría con Id {id} no encontrada.");

        // Validar nombre único (excluyendo el registro actual)
        var existing = await _repository.FindAsync(
            c => c.EmpresaId == entity.EmpresaId && c.Nombre == dto.Nombre && c.Id != id, ct);
        if (existing.Count > 0)
            return Result<CategoriaProductoDto>.Failure($"Ya existe otra categoría con nombre '{dto.Nombre}'.");

        entity.Nombre = dto.Nombre;
        entity.Descripcion = dto.Descripcion;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CategoriaProductoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Categoría con Id {id} no encontrada.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static CategoriaProductoDto MapToDto(CategoriaProducto e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}

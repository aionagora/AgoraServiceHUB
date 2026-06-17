namespace AgoraHub360.ERP.Application.Services;

using System;
using System.Linq;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class AlmacenService : IAlmacenService
{
    private readonly IRepository<Almacen> _repository;
    private readonly IRepository<Sucursal> _sucursalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AlmacenService(
        IRepository<Almacen> repository,
        IRepository<Sucursal> sucursalRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _sucursalRepository = sucursalRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<AlmacenDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<AlmacenDto>>.Failure("No existe empresa activa en la sesión.");

        var almacenes = await _repository.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var sucursales = await _sucursalRepository.FindAsync(s => s.EmpresaId == empresaId.Value, ct);

        var dtos = almacenes.Select(a => new AlmacenDto
        {
            Id = a.Id,
            Codigo = a.Codigo,
            Nombre = a.Nombre,
            SucursalId = a.SucursalId,
            SucursalNombre = a.SucursalId.HasValue 
                ? sucursales.FirstOrDefault(s => s.Id == a.SucursalId.Value)?.Nombre
                : null,
            Direccion = a.Direccion,
            Responsable = a.Responsable,
            Telefono = a.Telefono,
            Activo = a.Activo,
            FechaCreacion = a.FechaCreacion
        }).ToList();

        return Result<IReadOnlyList<AlmacenDto>>.Success(dtos.AsReadOnly());
    }

    public async Task<Result<AlmacenDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AlmacenDto>.Failure("No existe empresa activa en la sesión.");

        var almacenes = await _repository.FindAsync(a => a.Id == id && a.EmpresaId == empresaId.Value, ct);
        var entity = almacenes.FirstOrDefault();
        if (entity is null)
            return Result<AlmacenDto>.Failure("Almacén no encontrado.");
        return Result<AlmacenDto>.Success(MapToDto(entity));
    }

    public async Task<Result<AlmacenDto>> CreateAsync(CreateAlmacenDto dto, CancellationToken ct = default)
    {
        // 1. Validar empresa activa
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AlmacenDto>.Failure("No existe empresa activa en la sesión.");

        // 2. Validar campos requeridos
        if (string.IsNullOrWhiteSpace(dto.Codigo))
            return Result<AlmacenDto>.Failure("El código del almacén es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<AlmacenDto>.Failure("El nombre del almacén es obligatorio.");

        // 3. Validar código único por empresa
        var existing = await _repository.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Codigo == dto.Codigo.Trim(), ct);
        if (existing.Count > 0)
            return Result<AlmacenDto>.Failure($"Ya existe un almacén con el código {dto.Codigo} en la empresa activa.");

        // 4. Si se selecciona SucursalId, validar que pertenece a la empresa activa y está activa
        int? validatedSucursalId = null;
        if (dto.SucursalId.HasValue && dto.SucursalId.Value > 0)
        {
            var sucursal = await _sucursalRepository.GetByIdAsync(dto.SucursalId.Value, ct);
            if (sucursal == null)
                return Result<AlmacenDto>.Failure("La sucursal seleccionada no existe.");

            if (sucursal.EmpresaId != empresaId.Value)
                return Result<AlmacenDto>.Failure("La sucursal seleccionada no pertenece a la empresa activa.");

            if (!sucursal.Activo)
                return Result<AlmacenDto>.Failure("La sucursal seleccionada no está activa.");

            validatedSucursalId = dto.SucursalId.Value;
        }

        // 5. Crear entidad con EmpresaId del JWT (NO del DTO)
        var entity = new Almacen
        {
            Codigo = dto.Codigo.Trim(),
            Nombre = dto.Nombre.Trim(),
            Direccion = dto.Direccion?.Trim(),
            Responsable = dto.Responsable?.Trim(),
            Telefono = dto.Telefono?.Trim(),
            EmpresaId = empresaId.Value,
            SucursalId = validatedSucursalId
        };

        try
        {
            await _repository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<AlmacenDto>.Success(MapToDto(entity));
        }
        catch (Exception ex)
        {
            // Capturar errores de BD y devolver mensaje controlado
            var innerMsg = ex.InnerException?.Message ?? ex.Message;

            if (innerMsg.Contains("IX_Almacenes_EmpresaId_Codigo") || innerMsg.Contains("duplicate key"))
                return Result<AlmacenDto>.Failure($"Ya existe un almacén con el código {dto.Codigo} en la empresa activa.");

            if (innerMsg.Contains("FK_Almacenes_Sucursales"))
                return Result<AlmacenDto>.Failure("La sucursal seleccionada no es válida o no pertenece a la empresa activa.");

            return Result<AlmacenDto>.Failure($"Error al guardar el almacén: {innerMsg}");
        }
    }

    public async Task<Result<AlmacenDto>> UpdateAsync(int id, UpdateAlmacenDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<AlmacenDto>.Failure($"Almacén con Id {id} no encontrado.");

        // Validar código único (excluyendo el registro actual)
        var existing = await _repository.FindAsync(
            a => a.EmpresaId == entity.EmpresaId && a.Codigo == dto.Codigo && a.Id != id, ct);
        if (existing.Count > 0)
            return Result<AlmacenDto>.Failure($"Ya existe otro almacén con código '{dto.Codigo}'.");

        entity.Codigo = dto.Codigo;
        entity.Nombre = dto.Nombre;
        entity.Direccion = dto.Direccion;
        entity.Responsable = dto.Responsable;
        entity.Telefono = dto.Telefono;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AlmacenDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Almacén con Id {id} no encontrado.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static AlmacenDto MapToDto(Almacen e) => new()
    {
        Id = e.Id,
        Codigo = e.Codigo,
        Nombre = e.Nombre,
        SucursalId = e.SucursalId,
        SucursalNombre = e.Sucursal?.Nombre,
        Direccion = e.Direccion,
        Responsable = e.Responsable,
        Telefono = e.Telefono,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}

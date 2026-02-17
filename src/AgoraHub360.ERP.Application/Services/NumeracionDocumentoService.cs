namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;

public class NumeracionDocumentoService : INumeracionDocumentoService
{
    private readonly IRepository<NumeracionDocumento> _repository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUserService _currentUser;

    public NumeracionDocumentoService(
        IRepository<NumeracionDocumento> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;

    private readonly ICurrentUserService _currentUserService;

    public NumeracionDocumentoService(
        IRepository<NumeracionDocumento> repository, 
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;

    }

    public async Task<Result<IReadOnlyList<NumeracionDocumentoDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<NumeracionDocumentoDto>>.Failure("No se pudo determinar la empresa activa del usuario.");

        var items = await _repository.FindAsync(n => n.EmpresaId == empresaId.Value, ct);
        return Result<IReadOnlyList<NumeracionDocumentoDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<NumeracionDocumentoDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<NumeracionDocumentoDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<NumeracionDocumentoDto>.Failure("Numeración no encontrada.");

        // Verificar que pertenece a la empresa del usuario
        if (entity.EmpresaId != empresaId.Value)
            return Result<NumeracionDocumentoDto>.Failure("No tiene permisos para acceder a esta numeración.");

        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<NumeracionDocumentoDto>> CreateAsync(CreateNumeracionDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<NumeracionDocumentoDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        // Validar unicidad de TipoDocumento en la empresa
        var existing = await _repository.FindAsync(
            n => n.EmpresaId == empresaId.Value && n.TipoDocumento == dto.TipoDocumento, ct);
        if (existing.Any())
            return Result<NumeracionDocumentoDto>.Failure(
                $"Ya existe una numeración para el tipo '{dto.TipoDocumento}' en esta empresa.");

        var entity = new NumeracionDocumento
        {
            TipoDocumento = dto.TipoDocumento.ToUpperInvariant(),
            Descripcion = dto.Descripcion,
            Prefijo = dto.Prefijo,
            SiguienteNumero = dto.SiguienteNumero,
            Digitos = dto.Digitos,
         EmpresaId = _currentUser.EmpresaId ?? 0

            EmpresaId = empresaId.Value,
            Activo = true

        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<NumeracionDocumentoDto>> UpdateAsync(int id, UpdateNumeracionDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<NumeracionDocumentoDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<NumeracionDocumentoDto>.Failure("Numeración no encontrada.");

        // Verificar que pertenece a la empresa del usuario
        if (entity.EmpresaId != empresaId.Value)
            return Result<NumeracionDocumentoDto>.Failure("No tiene permisos para modificar esta numeración.");

        entity.Descripcion = dto.Descripcion;
        entity.Prefijo = dto.Prefijo;
        entity.SiguienteNumero = dto.SiguienteNumero;
        entity.Digitos = dto.Digitos;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure("Numeración no encontrada.");

        // Verificar que pertenece a la empresa del usuario
        if (entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("No tiene permisos para eliminar esta numeración.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static NumeracionDocumentoDto MapToDto(NumeracionDocumento n) => new()
    {
        Id = n.Id,
        TipoDocumento = n.TipoDocumento,
        Descripcion = n.Descripcion,
        Prefijo = n.Prefijo,
        SiguienteNumero = n.SiguienteNumero,
        Digitos = n.Digitos,
        EmpresaId = n.EmpresaId,
        Activo = n.Activo
    };
}

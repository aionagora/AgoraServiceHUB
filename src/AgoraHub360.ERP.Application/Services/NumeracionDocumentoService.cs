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

    public NumeracionDocumentoService(IRepository<NumeracionDocumento> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<NumeracionDocumentoDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<NumeracionDocumentoDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<NumeracionDocumentoDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<NumeracionDocumentoDto>.Failure("Numeración no encontrada.");

        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<NumeracionDocumentoDto>> CreateAsync(CreateNumeracionDto dto, CancellationToken ct = default)
    {
        // Validar unicidad de TipoDocumento en la empresa (filtro global de tenant aplica)
        var existing = await _repository.FindAsync(n => n.TipoDocumento == dto.TipoDocumento, ct);
        if (existing.Any())
            return Result<NumeracionDocumentoDto>.Failure(
                $"Ya existe una numeración para el tipo '{dto.TipoDocumento}' en esta empresa.");

        var entity = new NumeracionDocumento
        {
            TipoDocumento = dto.TipoDocumento.ToUpperInvariant(),
            Descripcion = dto.Descripcion,
            Prefijo = dto.Prefijo,
            SiguienteNumero = dto.SiguienteNumero,
            Digitos = dto.Digitos
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<NumeracionDocumentoDto>> UpdateAsync(int id, UpdateNumeracionDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<NumeracionDocumentoDto>.Failure("Numeración no encontrada.");

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
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure("Numeración no encontrada.");

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

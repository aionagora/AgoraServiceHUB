namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Parametro;

public class ParametroSistemaService : IParametroSistemaService
{
    private readonly IRepository<ParametroSistema> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ParametroSistemaService(
        IRepository<ParametroSistema> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<ParametroSistemaDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetByCategoriaAsync(string categoria, CancellationToken ct = default)
    {
        var items = await _repository.FindAsync(p => p.Categoria == categoria, ct);
        return Result<IReadOnlyList<ParametroSistemaDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<ParametroSistemaDto>> GetByClaveAsync(string clave, CancellationToken ct = default)
    {
        var items = await _repository.FindAsync(p => p.Clave == clave, ct);
        var entity = items.FirstOrDefault();
        if (entity is null)
            return Result<ParametroSistemaDto>.Failure($"Parámetro '{clave}' no encontrado.");

        return Result<ParametroSistemaDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ParametroSistemaDto>> UpsertAsync(UpsertParametroDto dto, CancellationToken ct = default)
    {
        var existing = (await _repository.FindAsync(p => p.Clave == dto.Clave, ct)).FirstOrDefault();

        if (existing is not null)
        {
            existing.Valor = dto.Valor;
            existing.Descripcion = dto.Descripcion;
            existing.Categoria = dto.Categoria;
            existing.TipoDato = dto.TipoDato;
            await _repository.UpdateAsync(existing, ct);
        }
        else
        {
            existing = new ParametroSistema
            {
                Clave = dto.Clave,
                Valor = dto.Valor,
                Descripcion = dto.Descripcion,
                Categoria = dto.Categoria,
                TipoDato = dto.TipoDato,
                EmpresaId = _currentUser.EmpresaId ?? 0
            };
            await _repository.AddAsync(existing, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ParametroSistemaDto>.Success(MapToDto(existing));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure("Parámetro no encontrado.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ParametroSistemaDto MapToDto(ParametroSistema p) => new()
    {
        Id = p.Id,
        Clave = p.Clave,
        Valor = p.Valor,
        Descripcion = p.Descripcion,
        Categoria = p.Categoria,
        TipoDato = p.TipoDato,
        EmpresaId = p.EmpresaId,
        Activo = p.Activo
    };
}

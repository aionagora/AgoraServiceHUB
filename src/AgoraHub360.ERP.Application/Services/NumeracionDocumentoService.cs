namespace AgoraHub360.ERP.Application.Services;

using System.Collections.Concurrent;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;

public class NumeracionDocumentoService : INumeracionDocumentoService
{
    private readonly IRepository<NumeracionDocumento> _repository;
    private readonly IRepository<Sucursal> _sucursalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> SerieLocks = new();

    public NumeracionDocumentoService(
        IRepository<NumeracionDocumento> repository,
        IRepository<Sucursal> sucursalRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _sucursalRepository = sucursalRepository;
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

    public async Task<Result<string>> GenerarSiguienteNumeroAsync(string tipoDocumento, long sucursalId, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<string>.Failure("No se pudo determinar la empresa activa del usuario.");

        var tipoNormalizado = (tipoDocumento ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(tipoNormalizado))
            return Result<string>.Failure("El tipo de documento es obligatorio para generar numeración.");

        if (sucursalId <= 0 || sucursalId > int.MaxValue)
            return Result<string>.Failure("Sucursal inválida para la generación de numeración.");

        var sucursal = await _sucursalRepository.GetByIdAsync((int)sucursalId, ct);
        if (sucursal is null || sucursal.EmpresaId != empresaId.Value || !sucursal.Activo)
            return Result<string>.Failure("La sucursal no existe o no pertenece a la empresa activa.");

        var codigoSucursal = ResolveCodigoSucursalDocumental(sucursal);
        var serieKey = $"{empresaId.Value}:{tipoNormalizado}:{sucursal.Id}";
        var semaphore = SerieLocks.GetOrAdd(serieKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(ct);
        try
        {
            var numeracion = (await _repository.FindAsync(
                    n => n.EmpresaId == empresaId.Value
                         && n.TipoDocumento == tipoNormalizado
                         && n.SucursalId == sucursal.Id,
                    ct))
                .FirstOrDefault();

            if (numeracion is null)
            {
                numeracion = new NumeracionDocumento
                {
                    EmpresaId = empresaId.Value,
                    SucursalId = sucursal.Id,
                    TipoDocumento = tipoNormalizado,
                    Descripcion = $"Numeración {tipoNormalizado} sucursal {codigoSucursal}",
                    Prefijo = $"{tipoNormalizado}-{codigoSucursal}",
                    SiguienteNumero = 1,
                    Digitos = 6,
                    Activo = true
                };

                await _repository.AddAsync(numeracion, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            if (!numeracion.Activo)
                numeracion.Activo = true;

            var numeroEmitido = numeracion.SiguienteNumero <= 0 ? 1 : numeracion.SiguienteNumero;
            var numeroFormateado = $"{numeracion.Prefijo}-{numeroEmitido:D6}";

            numeracion.SiguienteNumero = numeroEmitido + 1;

            await _repository.UpdateAsync(numeracion, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<string>.Success(numeroFormateado);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"No se pudo generar la numeración documental: {ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string ResolveCodigoSucursalDocumental(Sucursal sucursal)
    {
        var codigoBruto =
            FirstNonEmpty(
                sucursal.PrefijoDocumental,
                sucursal.CodigoSucursalFiscal,
                sucursal.Sigla,
                sucursal.CodigoInterno,
                sucursal.Codigo)
            ?? "SUC00";

        var normalizado = codigoBruto
            .Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizado))
            return "SUC00";

        return normalizado.Length > 10
            ? normalizado[..10]
            : normalizado;
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

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

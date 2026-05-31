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
        var sucursales = await _sucursalRepository.FindAsync(s => s.EmpresaId == empresaId.Value, ct);
        var sucursalMap = sucursales.ToDictionary(s => s.Id, s => s.Nombre);

        return Result<IReadOnlyList<NumeracionDocumentoDto>>.Success(
            items.Select(n => MapToDto(n, ResolveSucursalNombre(n.SucursalId, sucursalMap))).ToList().AsReadOnly());
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

        var sucursalNombre = await ResolveSucursalNombreAsync(entity.SucursalId, empresaId.Value, ct);
        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity, sucursalNombre));
    }

    public async Task<Result<NumeracionDocumentoDto>> CreateAsync(CrearNumeracionDocumentoRequestDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<NumeracionDocumentoDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        var validacion = await ValidarDatosAsync(dto.TipoDocumento, dto.Prefijo, dto.SiguienteNumero, dto.LongitudNumero, dto.SucursalId, empresaId.Value, ct);
        if (!validacion.IsSuccess)
            return Result<NumeracionDocumentoDto>.Failure(validacion.Error!);

        var tipoDocumento = dto.TipoDocumento.Trim().ToUpperInvariant();
        var prefijo = dto.Prefijo.Trim().ToUpperInvariant();

        // Validar unicidad por Empresa + Sucursal + TipoDocumento
        var existing = await _repository.FindAsync(
            n => n.EmpresaId == empresaId.Value
                 && n.SucursalId == dto.SucursalId
                 && n.TipoDocumento == tipoDocumento,
            ct);

        if (existing.Any())
            return Result<NumeracionDocumentoDto>.Failure(
                $"Ya existe una numeración para el tipo '{tipoDocumento}' en la sucursal seleccionada.");

        var entity = new NumeracionDocumento
        {
            TipoDocumento = tipoDocumento,
            Descripcion = dto.Descripcion.Trim(),
            SucursalId = dto.SucursalId,
            Prefijo = prefijo,
            SiguienteNumero = dto.SiguienteNumero,
            Digitos = dto.LongitudNumero,
            EmpresaId = empresaId.Value,
            Activo = dto.Activo
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var sucursalNombre = await ResolveSucursalNombreAsync(entity.SucursalId, empresaId.Value, ct);
        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity, sucursalNombre));
    }

    public async Task<Result<NumeracionDocumentoDto>> UpdateAsync(int id, ActualizarNumeracionDocumentoRequestDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<NumeracionDocumentoDto>.Failure("No se pudo determinar la empresa activa del usuario.");

        var validacion = await ValidarDatosAsync(dto.TipoDocumento, dto.Prefijo, dto.SiguienteNumero, dto.LongitudNumero, dto.SucursalId, empresaId.Value, ct);
        if (!validacion.IsSuccess)
            return Result<NumeracionDocumentoDto>.Failure(validacion.Error!);

        var tipoDocumento = dto.TipoDocumento.Trim().ToUpperInvariant();
        var prefijo = dto.Prefijo.Trim().ToUpperInvariant();

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<NumeracionDocumentoDto>.Failure("Numeración no encontrada.");

        // Verificar que pertenece a la empresa del usuario
        if (entity.EmpresaId != empresaId.Value)
            return Result<NumeracionDocumentoDto>.Failure("No tiene permisos para modificar esta numeración.");

        var fueUsada = entity.SiguienteNumero > 1;

        if (fueUsada && entity.SiguienteNumero > dto.SiguienteNumero)
            return Result<NumeracionDocumentoDto>.Failure("No se puede disminuir el siguiente número porque ya se emitieron documentos.");

        if (fueUsada && !string.Equals(entity.TipoDocumento, tipoDocumento, StringComparison.OrdinalIgnoreCase))
            return Result<NumeracionDocumentoDto>.Failure("No se puede cambiar el tipo de documento cuando la numeración ya fue usada.");

        if (fueUsada && entity.SucursalId != dto.SucursalId)
            return Result<NumeracionDocumentoDto>.Failure("No se puede cambiar la sucursal cuando la numeración ya fue usada.");

        if (fueUsada && !string.Equals(entity.Prefijo, prefijo, StringComparison.OrdinalIgnoreCase))
            return Result<NumeracionDocumentoDto>.Failure("No se puede cambiar el prefijo cuando la numeración ya fue usada.");

        var duplicado = await _repository.FindAsync(
            n => n.EmpresaId == empresaId.Value
                 && n.Id != id
                 && n.SucursalId == dto.SucursalId
                 && n.TipoDocumento == tipoDocumento,
            ct);

        if (duplicado.Any())
            return Result<NumeracionDocumentoDto>.Failure("Ya existe una numeración con el mismo tipo de documento para la sucursal seleccionada.");

        entity.TipoDocumento = tipoDocumento;
        entity.Descripcion = dto.Descripcion.Trim();
        entity.SucursalId = dto.SucursalId;
        entity.Prefijo = prefijo;
        entity.SiguienteNumero = dto.SiguienteNumero;
        entity.Digitos = dto.LongitudNumero;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var sucursalNombre = await ResolveSucursalNombreAsync(entity.SucursalId, empresaId.Value, ct);
        return Result<NumeracionDocumentoDto>.Success(MapToDto(entity, sucursalNombre));
    }

    public async Task<Result<bool>> ActivarAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure("Numeración no encontrada.");

        // Verificar que pertenece a la empresa del usuario
        if (entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("No tiene permisos para modificar esta numeración.");

        if (!entity.Activo)
        {
            entity.Activo = true;
            await _repository.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DesactivarAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa del usuario.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure("Numeración no encontrada.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("No tiene permisos para modificar esta numeración.");

        if (entity.Activo)
        {
            entity.Activo = false;
            await _repository.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

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

    private async Task<Result<bool>> ValidarDatosAsync(
        string tipoDocumento,
        string prefijo,
        int siguienteNumero,
        int longitudNumero,
        int? sucursalId,
        int empresaId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tipoDocumento))
            return Result<bool>.Failure("El tipo de documento es obligatorio.");

        if (string.IsNullOrWhiteSpace(prefijo))
            return Result<bool>.Failure("El prefijo es obligatorio.");

        if (siguienteNumero < 1)
            return Result<bool>.Failure("El siguiente número debe ser mayor o igual a 1.");

        if (longitudNumero < 1 || longitudNumero > 10)
            return Result<bool>.Failure("La longitud del número debe estar entre 1 y 10.");

        if (sucursalId.HasValue)
        {
            if (sucursalId.Value <= 0)
                return Result<bool>.Failure("La sucursal seleccionada es inválida.");

            var sucursal = await _sucursalRepository.GetByIdAsync(sucursalId.Value, ct);
            if (sucursal is null || sucursal.EmpresaId != empresaId)
                return Result<bool>.Failure("La sucursal no pertenece a la empresa activa.");
        }

        return Result<bool>.Success(true);
    }

    private async Task<string?> ResolveSucursalNombreAsync(int? sucursalId, int empresaId, CancellationToken ct)
    {
        if (!sucursalId.HasValue)
            return null;

        var sucursal = await _sucursalRepository.GetByIdAsync(sucursalId.Value, ct);
        if (sucursal is null || sucursal.EmpresaId != empresaId)
            return null;

        return sucursal.Nombre;
    }

    private static string? ResolveSucursalNombre(int? sucursalId, IReadOnlyDictionary<int, string> sucursalMap)
    {
        if (!sucursalId.HasValue)
            return null;

        return sucursalMap.TryGetValue(sucursalId.Value, out var nombre)
            ? nombre
            : null;
    }

    private static NumeracionDocumentoDto MapToDto(NumeracionDocumento n, string? sucursalNombre = null) => new()
    {
        Id = n.Id,
        SucursalId = n.SucursalId,
        SucursalNombre = sucursalNombre,
        TipoDocumento = n.TipoDocumento,
        Descripcion = n.Descripcion,
        Prefijo = n.Prefijo,
        SiguienteNumero = n.SiguienteNumero,
        Digitos = n.Digitos,
        EmpresaId = n.EmpresaId,
        Activo = n.Activo
    };
}

namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.LOG;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Logistica;

public class HojaRutaService : IHojaRutaService
{
    private readonly IRepository<HojaRuta> _repo;
    private readonly IRepository<HojaRutaHistorial> _historialRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    // Tipos principales válidos
    private static readonly HashSet<string> TiposOPValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "INTERNA", "CLIENTE"
    };

    // Sub-tipos válidos por tipo principal
    private static readonly Dictionary<string, HashSet<string>> SubTiposValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["INTERNA"] = new(StringComparer.OrdinalIgnoreCase) { "IMPORTACION", "TRASPASO_INTERNO", "DEVOLUCION", "AJUSTE" },
        ["CLIENTE"] = new(StringComparer.OrdinalIgnoreCase) { "ENTREGA", "RECOJO", "DEVOLUCION_CLI" },
    };

    // Transiciones válidas por tipo:
    // INTERNA: BORRADOR ? EN_PROCESO (flujo aduanero) ? COMPLETADO
    // CLIENTE: BORRADOR ? EN_PROCESO (flujo entrega)  ? COMPLETADO
    private static readonly Dictionary<(string Estado, string SubEstado), List<(string Estado, string SubEstado)>> TransicionesValidas = new()
    {
        // ?? Compartidas ??????????????????????????????????????????????????????
        [("BORRADOR",    "INICIADO")]      = new() { ("EN_PROCESO", "EMBARCADO"),  ("EN_PROCESO", "PREPARANDO"), ("ANULADO", "ANULADO") },
        // ?? INTERNA (flujo aduanero) ?????????????????????????????????????????
        [("EN_PROCESO",  "EMBARCADO")]     = new() { ("EN_PROCESO", "EN_ADUANA"),  ("ANULADO", "ANULADO") },
        [("EN_PROCESO",  "EN_ADUANA")]     = new() { ("EN_PROCESO", "AFORO"),      ("EN_PROCESO", "LEVANTE"), ("ANULADO", "ANULADO") },
        [("EN_PROCESO",  "AFORO")]         = new() { ("EN_PROCESO", "LEVANTE"),    ("ANULADO", "ANULADO") },
        [("EN_PROCESO",  "LEVANTE")]       = new() { ("EN_PROCESO", "EN_RECEPCION"), ("ANULADO", "ANULADO") },
        [("EN_PROCESO",  "EN_RECEPCION")]  = new() { ("COMPLETADO", "CERRADO"),    ("ANULADO", "ANULADO") },
        // ?? CLIENTE (flujo entrega) ??????????????????????????????????????????
        [("EN_PROCESO",  "PREPARANDO")]    = new() { ("EN_PROCESO", "EN_RUTA"),    ("ANULADO", "ANULADO") },
        [("EN_PROCESO",  "EN_RUTA")]       = new() { ("COMPLETADO", "ENTREGADO"),  ("EN_PROCESO", "NO_ENTREGADO"), ("ANULADO", "ANULADO") },
        [("EN_PROCESO",  "NO_ENTREGADO")]  = new() { ("EN_PROCESO", "EN_RUTA"),    ("ANULADO", "ANULADO") },
        [("COMPLETADO",  "ENTREGADO")]     = new() { ("COMPLETADO", "CERRADO") },
    };

    public HojaRutaService(
        IRepository<HojaRuta> repo,
        IRepository<HojaRutaHistorial> historialRepo,
        IRepository<NumeracionDocumento> numRepo,
        IRepository<Almacen> almacenRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _historialRepo = historialRepo;
        _numRepo = numRepo;
        _almacenRepo = almacenRepo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<HojaRutaDto>>> GetAllAsync(
        string? estado, string? subEstado, string? tipoOP,
        DateTime? desde, DateTime? hasta, string? search,
        CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<HojaRutaDto>>.Failure("No se pudo determinar la empresa activa.");

        var items = await _repo.FindAsync(h => h.EmpresaId == empresaId.Value, ct);
        var query = items.AsEnumerable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(h => h.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(subEstado))
            query = query.Where(h => h.SubEstado.Equals(subEstado, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(tipoOP))
            query = query.Where(h => h.TipoOP.Equals(tipoOP, StringComparison.OrdinalIgnoreCase));
        if (desde.HasValue)
            query = query.Where(h => h.FechaDocumento >= desde.Value.Date);
        if (hasta.HasValue)
            query = query.Where(h => h.FechaDocumento <= hasta.Value.Date);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(h =>
                h.NumeroHojaRuta.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (h.Observaciones ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var almacenDict = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        var result = query
            .OrderByDescending(h => h.FechaDocumento)
            .ThenByDescending(h => h.HojaRutaId)
            .Select(h => MapToDto(h, almacenDict))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<HojaRutaDto>>.Success(result);
    }

    public async Task<Result<HojaRutaDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaRutaDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId.Value)
            return Result<HojaRutaDto>.Failure("Hoja de Ruta no encontrada.");

        // Cargar historial
        var historial = await _historialRepo.FindAsync(h => h.HojaRutaId == id, ct);
        entity.Historial = historial.OrderByDescending(h => h.FechaCambio).ToList();

        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var almacenDict = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        return Result<HojaRutaDto>.Success(MapToDto(entity, almacenDict));
    }

    public async Task<Result<HojaRutaDto>> CreateAsync(CreateHojaRutaDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaRutaDto>.Failure("No se pudo determinar la empresa activa.");

        if (!TiposOPValidos.Contains(dto.TipoOP))
            return Result<HojaRutaDto>.Failure($"Tipo de operación inválido. Valores permitidos: {string.Join(", ", TiposOPValidos)}");

        if (!string.IsNullOrEmpty(dto.SubTipo) &&
            SubTiposValidos.TryGetValue(dto.TipoOP, out var subTiposPermitidos) &&
            !subTiposPermitidos.Contains(dto.SubTipo))
            return Result<HojaRutaDto>.Failure(
                $"Sub-tipo '{dto.SubTipo}' no válido para {dto.TipoOP}. " +
                $"Valores permitidos: {string.Join(", ", subTiposPermitidos)}");

        if (dto.AlmacenOrigenId <= 0)
            return Result<HojaRutaDto>.Failure("Debe seleccionar un almacén de origen.");

        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenOrigenId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<HojaRutaDto>.Failure("Almacén de origen no válido.");

        // Número 100% dinámico desde core.NumeracionesDocumento
        var numeroResult = await GenerarNumeroAsync(empresaId.Value, ct);
        if (!numeroResult.IsSuccess)
            return Result<HojaRutaDto>.Failure(numeroResult.Error!);

        var entity = new HojaRuta
        {
            EmpresaId        = empresaId.Value,
            NumeroHojaRuta   = numeroResult.Value!,
            TipoOP           = dto.TipoOP.ToUpperInvariant(),
            SubTipo          = dto.SubTipo?.ToUpperInvariant() ?? string.Empty,
            AlmacenOrigenId  = dto.AlmacenOrigenId,
            AlmacenDestinoId = dto.AlmacenDestinoId,
            ProveedorCliente = dto.ProveedorCliente,
            DireccionEntrega = dto.DireccionEntrega,
            ContactoCliente  = dto.ContactoCliente,
            ResponsableUsuario = _currentUser.UserName ?? "Sistema",
            FechaRegistro    = DateTime.UtcNow,
            FechaDocumento   = dto.FechaDocumento,
            ETA              = dto.ETA,
            Estado           = "BORRADOR",
            SubEstado        = "INICIADO",
            Observaciones    = dto.Observaciones,
            OrdenPedidoId    = dto.OrdenPedidoId,
            Activo           = true
        };

        await _repo.AddAsync(entity, ct);

        var historial = new HojaRutaHistorial
        {
            HojaRutaId        = entity.HojaRutaId,
            EstadoAnterior    = "",
            SubEstadoAnterior = "",
            EstadoNuevo       = "BORRADOR",
            SubEstadoNuevo    = "INICIADO",
            FechaCambio       = DateTime.UtcNow,
            Usuario           = _currentUser.UserName ?? "Sistema",
            Observaciones     = "Hoja de Ruta creada."
        };
        await _historialRepo.AddAsync(historial, ct);

        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.HojaRutaId, ct);
    }

    public async Task<Result<HojaRutaDto>> UpdateAsync(long id, UpdateHojaRutaDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaRutaDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId.Value)
            return Result<HojaRutaDto>.Failure("Hoja de Ruta no encontrada.");

        if (entity.Estado != "BORRADOR")
            return Result<HojaRutaDto>.Failure("Solo se pueden editar hojas de ruta en estado BORRADOR.");

        if (!TiposOPValidos.Contains(dto.TipoOP))
            return Result<HojaRutaDto>.Failure($"Tipo de operación inválido. Valores permitidos: {string.Join(", ", TiposOPValidos)}");

        if (!string.IsNullOrEmpty(dto.SubTipo) &&
            SubTiposValidos.TryGetValue(dto.TipoOP, out var subTiposPermitidos) &&
            !subTiposPermitidos.Contains(dto.SubTipo))
            return Result<HojaRutaDto>.Failure(
                $"Sub-tipo '{dto.SubTipo}' no válido para {dto.TipoOP}. " +
                $"Valores permitidos: {string.Join(", ", subTiposPermitidos)}");

        if (dto.AlmacenOrigenId <= 0)
            return Result<HojaRutaDto>.Failure("Debe seleccionar un almacén de origen.");

        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenOrigenId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<HojaRutaDto>.Failure("Almacén de origen no válido.");

        entity.TipoOP           = dto.TipoOP.ToUpperInvariant();
        entity.SubTipo          = dto.SubTipo?.ToUpperInvariant() ?? string.Empty;
        entity.AlmacenOrigenId  = dto.AlmacenOrigenId;
        entity.AlmacenDestinoId = dto.AlmacenDestinoId;
        entity.ProveedorCliente = dto.ProveedorCliente;
        entity.DireccionEntrega = dto.DireccionEntrega;
        entity.ContactoCliente  = dto.ContactoCliente;
        entity.FechaDocumento   = dto.FechaDocumento;
        entity.ETA              = dto.ETA;
        entity.Observaciones    = dto.Observaciones;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<Result<HojaRutaDto>> CambiarEstadoAsync(long id, CambiarEstadoHojaRutaDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaRutaDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId.Value)
            return Result<HojaRutaDto>.Failure("Hoja de Ruta no encontrada.");

        var claveActual = (entity.Estado, entity.SubEstado);
        var destino = (dto.NuevoEstado.ToUpperInvariant(), dto.NuevoSubEstado.ToUpperInvariant());

        if (!TransicionesValidas.TryGetValue(claveActual, out var destinos) || !destinos.Contains(destino))
            return Result<HojaRutaDto>.Failure(
                $"Transición no permitida: ({entity.Estado}/{entity.SubEstado}) ? ({destino.Item1}/{destino.Item2}).");

        // No se puede anular una hoja ya COMPLETADA o ANULADA
        if (entity.Estado == "COMPLETADO" || entity.Estado == "ANULADO")
            return Result<HojaRutaDto>.Failure($"No se puede cambiar estado de una hoja de ruta {entity.Estado}.");

        var historial = new HojaRutaHistorial
        {
            HojaRutaId = id,
            EstadoAnterior = entity.Estado,
            SubEstadoAnterior = entity.SubEstado,
            EstadoNuevo = destino.Item1,
            SubEstadoNuevo = destino.Item2,
            FechaCambio = DateTime.UtcNow,
            Usuario = _currentUser.UserName ?? "Sistema",
            Observaciones = dto.Observaciones
        };

        entity.Estado = destino.Item1;
        entity.SubEstado = destino.Item2;

        await _repo.UpdateAsync(entity, ct);
        await _historialRepo.AddAsync(historial, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Hoja de Ruta no encontrada.");

        if (entity.Estado != "BORRADOR")
            return Result<bool>.Failure("Solo se pueden eliminar hojas de ruta en estado BORRADOR.");

        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<List<HojaRutaDto>>> GetByOrdenPedidoAsync(
        long ordenPedidoId, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<List<HojaRutaDto>>.Failure("No se pudo determinar la empresa activa.");

        var items = await _repo.FindAsync(
            h => h.EmpresaId == empresaId.Value && h.OrdenPedidoId == ordenPedidoId, ct);

        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var almacenDict = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        // Cargar historial para cada HR
        var dtos = new List<HojaRutaDto>();
        foreach (var h in items.OrderByDescending(x => x.FechaDocumento))
        {
            var historial = await _historialRepo.FindAsync(x => x.HojaRutaId == h.HojaRutaId, ct);
            h.Historial   = historial.OrderByDescending(x => x.FechaCambio).ToList();
            dtos.Add(MapToDto(h, almacenDict));
        }

        return Result<List<HojaRutaDto>>.Success(dtos);
    }

    // ?? Helpers ??????????????????????????????????????????????????????????????

    /// <summary>
    /// Lee prefijo, dígitos y correlativo 100% desde <c>core.NumeracionesDocumento</c>
    /// (TipoDocumento = 'HR', Activo = true). Sin ningún valor hardcodeado.
    /// Devuelve <see cref="Result{T}"/> con mensaje legible si la fila no existe o está inactiva,
    /// para que <see cref="CreateAsync"/> pueda propagarlo sin lanzar excepciones.
    /// </summary>
    private async Task<Result<string>> GenerarNumeroAsync(int empresaId, CancellationToken ct)
    {
        var nums = await _numRepo.FindAsync(
            n => n.EmpresaId == empresaId && n.TipoDocumento == "HR" && n.Activo, ct);
        var numeracion = nums.FirstOrDefault();

        if (numeracion is null)
            return Result<string>.Failure(
                "No existe una numeración activa para Hojas de Ruta (TipoDocumento = 'HR'). " +
                "Configúrela en Parámetros ? Numeración antes de crear una Hoja de Ruta.");

        // GenerarSiguiente() aplica: Prefijo + SiguienteNumero.PadLeft(Digitos, '0')
        // y avanza el contador internamente. Todo configurable desde la UI de Numeración.
        var numero = numeracion.GenerarSiguiente();
        await _numRepo.UpdateAsync(numeracion, ct);
        return Result<string>.Success(numero);
    }

    private static HojaRutaDto MapToDto(HojaRuta h, Dictionary<int, string> almacenes)
    {
        return new HojaRutaDto
        {
            HojaRutaId           = h.HojaRutaId,
            EmpresaId            = h.EmpresaId,
            NumeroHojaRuta       = h.NumeroHojaRuta,
            TipoOP               = h.TipoOP,
            SubTipo              = h.SubTipo,
            AlmacenOrigenId      = h.AlmacenOrigenId,
            AlmacenOrigenNombre  = almacenes.TryGetValue(h.AlmacenOrigenId, out var nOrigen) ? nOrigen : null,
            AlmacenDestinoId     = h.AlmacenDestinoId,
            AlmacenDestinoNombre = h.AlmacenDestinoId.HasValue && almacenes.TryGetValue(h.AlmacenDestinoId.Value, out var nDestino) ? nDestino : null,
            ProveedorCliente     = h.ProveedorCliente,
            DireccionEntrega     = h.DireccionEntrega,
            ContactoCliente      = h.ContactoCliente,
            ResponsableUsuario   = h.ResponsableUsuario,
            FechaRegistro        = h.FechaRegistro,
            FechaDocumento       = h.FechaDocumento,
            ETA                  = h.ETA,
            Estado               = h.Estado,
            SubEstado            = h.SubEstado,
            Observaciones        = h.Observaciones,
            Activo               = h.Activo,
            OrdenPedidoId        = h.OrdenPedidoId,
            Historial = h.Historial.Select(x => new HojaRutaHistorialDto
            {
                HojaRutaHistorialId = x.HojaRutaHistorialId,
                EstadoAnterior      = x.EstadoAnterior,
                SubEstadoAnterior   = x.SubEstadoAnterior,
                EstadoNuevo         = x.EstadoNuevo,
                SubEstadoNuevo      = x.SubEstadoNuevo,
                FechaCambio         = x.FechaCambio,
                Usuario             = x.Usuario,
                Observaciones       = x.Observaciones
            }).ToList()
        };
    }
}

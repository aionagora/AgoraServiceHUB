namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Notificacion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementación de <see cref="INotificacionService"/> basada en log.
/// Registra cada notificación en el log del sistema y utiliza <see cref="IMemoryCache"/>
/// para evitar duplicados: si ya se emitió la misma notificación (Tipo + ReferenciaId + EmpresaId)
/// en el día actual, el envío se omite silenciosamente.
/// </summary>
public class LogNotificacionService : INotificacionService
{
    private readonly ILogger<LogNotificacionService> _logger;
    private readonly IMemoryCache _cache;

    public LogNotificacionService(ILogger<LogNotificacionService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task EnviarAsync(CreateNotificacionDto dto, CancellationToken ct = default)
    {
        var hoyUtc = DateTime.UtcNow.Date;
        var cacheKey = $"notif:{dto.Tipo}:{dto.EmpresaId}:{dto.ReferenciaId}:{hoyUtc:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out _))
        {
            _logger.LogDebug(
                "Notificación duplicada omitida — Tipo={Tipo} ReferenciaId={ReferenciaId} EmpresaId={EmpresaId}",
                dto.Tipo, dto.ReferenciaId, dto.EmpresaId);
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "[NOTIFICACION] EmpresaId={EmpresaId} Tipo={Tipo} Prioridad={Prioridad} — {Titulo}: {Mensaje}",
            dto.EmpresaId, dto.Tipo, dto.Prioridad, dto.Titulo, dto.Mensaje);

        // Guardar en caché hasta fin del día UTC para deduplicación
        var expiracion = hoyUtc.AddDays(1) - DateTime.UtcNow;
        _cache.Set(cacheKey, true, expiracion);

        return Task.CompletedTask;
    }
}

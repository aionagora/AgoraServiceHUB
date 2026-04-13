namespace AgoraHub360.ERP.Api.BackgroundServices;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Servicio en background que, una vez al día a las 08:00 AM (UTC),
/// recorre todas las empresas activas y emite notificaciones para los
/// períodos contables abiertos próximos a cerrar.
/// </summary>
public class PeriodoContableNotificadorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PeriodoContableNotificadorService> _logger;

    /// <summary>Hora UTC a la que se ejecuta el chequeo diario.</summary>
    private static readonly TimeOnly HoraEjecucion = new(8, 0);

    public PeriodoContableNotificadorService(
        IServiceScopeFactory scopeFactory,
        ILogger<PeriodoContableNotificadorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "PeriodoContableNotificadorService iniciado. Ejecutará diariamente a las {Hora} UTC.",
            HoraEjecucion);

        while (!stoppingToken.IsCancellationRequested)
        {
            var espera = CalcularEsperaHastaProximaEjecucion();

            _logger.LogDebug(
                "Próxima verificación de períodos en {Minutos:F0} minutos.",
                espera.TotalMinutes);

            try
            {
                await Task.Delay(espera, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await EjecutarVerificacionAsync(stoppingToken);
        }

        _logger.LogInformation("PeriodoContableNotificadorService detenido.");
    }

    private async Task EjecutarVerificacionAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Iniciando verificación diaria de períodos contables próximos a cerrar.");

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var empresaRepo = scope.ServiceProvider.GetRequiredService<IRepository<Empresa>>();
            var notificacionPeriodo = scope.ServiceProvider
                .GetRequiredService<IPeriodoContableNotificacionService>();

            var empresas = await empresaRepo.FindAsync(e => e.Activo, ct);

            foreach (var empresa in empresas)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    await notificacionPeriodo.VerificarPeriodosProximosAsync(empresa.Id, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al verificar períodos de EmpresaId={EmpresaId}.", empresa.Id);
                }
            }

            _logger.LogInformation(
                "Verificación diaria completada. Empresas revisadas: {Total}.", empresas.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en la verificación diaria de períodos contables.");
        }
    }

    /// <summary>
    /// Calcula el tiempo de espera hasta la próxima ejecución diaria a <see cref="HoraEjecucion"/> UTC.
    /// </summary>
    private static TimeSpan CalcularEsperaHastaProximaEjecucion()
    {
        var ahora = DateTime.UtcNow;
        var proximaEjecucion = ahora.Date.Add(HoraEjecucion.ToTimeSpan());

        if (ahora >= proximaEjecucion)
            proximaEjecucion = proximaEjecucion.AddDays(1);

        return proximaEjecucion - ahora;
    }
}

namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Notificacion;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementación de <see cref="IPeriodoContableNotificacionService"/>.
/// Verifica períodos contables abiertos cuyo último día del mes esté próximo
/// y emite notificaciones a través de <see cref="INotificacionService"/>.
/// </summary>
public class PeriodoContableNotificacionService : IPeriodoContableNotificacionService
{
    /// <summary>Días restantes hasta fin de mes que activan la notificación.</summary>
    private const int DiasUmbral = 5;

    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly INotificacionService _notificacionService;
    private readonly ILogger<PeriodoContableNotificacionService> _logger;

    public PeriodoContableNotificacionService(
        IRepository<PeriodoContable> periodoRepo,
        INotificacionService notificacionService,
        ILogger<PeriodoContableNotificacionService> logger)
    {
        _periodoRepo = periodoRepo;
        _notificacionService = notificacionService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task VerificarPeriodosProximosAsync(int empresaId, CancellationToken ct = default)
    {
        var hoy = DateTime.UtcNow.Date;

        var periodos = await _periodoRepo.FindAsync(
            p => p.EmpresaId == empresaId && p.Activo && p.Estado == "Abierto", ct);

        foreach (var periodo in periodos)
        {
            // El último día del mes es la fecha de fin natural del período.
            var ultimoDiaMes = new DateTime(periodo.Anio, periodo.Mes,
                DateTime.DaysInMonth(periodo.Anio, periodo.Mes));

            var diasRestantes = (ultimoDiaMes - hoy).Days;

            if (diasRestantes <= DiasUmbral && diasRestantes >= 0)
            {
                var prioridad = diasRestantes <= 2 ? "Alta" : "Media";

                _logger.LogDebug(
                    "Período {Nombre} (EmpresaId={EmpresaId}) cierra en {Dias} día(s) — notificando.",
                    periodo.Nombre, empresaId, diasRestantes);

                await _notificacionService.EnviarAsync(new CreateNotificacionDto
                {
                    EmpresaId = empresaId,
                    Titulo = "Período contable próximo a cerrar",
                    Mensaje = $"El período {periodo.Nombre} cierra en {diasRestantes} día(s) ({ultimoDiaMes:dd/MM/yyyy}). Revise asientos pendientes en borrador.",
                    Tipo = "CONTABILIDAD_PERIODO_PROXIMO",
                    Prioridad = prioridad,
                    ReferenciaId = periodo.PeriodoContableId.ToString(),
                    ReferenciaTipo = "PeriodoContable"
                }, ct);
            }
        }
    }
}

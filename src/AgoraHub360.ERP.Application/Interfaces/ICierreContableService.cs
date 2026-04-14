namespace AgoraHub360.ERP.Application.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface ICierreContableService
{
    /// <summary>Cierre básico: cancela cuentas de resultado y registra el cierre (sin IUE ni reserva legal).</summary>
    Task<CierreContableDto> EjecutarCierreAsync(int empresaId, EjecutarCierreDto dto, CancellationToken ct);

    /// <summary>
    /// Cierre anual completo: valida períodos, calcula IUE, genera reserva legal,
    /// transfiere resultado a cuentas acumuladas y, opcionalmente, abre la nueva gestión.
    /// Todo en una única transacción con rollback total si falla cualquier paso.
    /// </summary>
    Task<CierreContableDto> EjecutarCierreAnualAsync(EjecutarCierreAnualDto dto, CancellationToken ct = default);

    Task<bool> ExisteCierreAsync(int empresaId, int gestion, CancellationToken ct);
    Task<CierreContableDto?> ObtenerCierreAsync(int empresaId, int gestion, CancellationToken ct);
    Task EjecutarAperturaAsync(int empresaId, int gestionNueva, CancellationToken ct);
}

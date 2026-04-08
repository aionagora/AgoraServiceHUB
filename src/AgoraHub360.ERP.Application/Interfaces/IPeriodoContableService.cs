namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface IPeriodoContableService
{
    /// <summary>Lista los períodos de un año (o todos si no se especifica).</summary>
    Task<Result<IReadOnlyList<PeriodoContableDto>>> GetAllAsync(int? anio = null, CancellationToken ct = default);

    /// <summary>Genera los 12 períodos de un año fiscal.</summary>
    Task<Result<int>> GenerarPeriodosAsync(int anio, CancellationToken ct = default);

    /// <summary>Cierra un período (no se podrán crear/contabilizar asientos en ese mes).</summary>
    Task<Result<PeriodoContableDto>> CerrarAsync(int id, CancellationToken ct = default);

    /// <summary>Reabre un período cerrado.</summary>
    Task<Result<PeriodoContableDto>> ReabrirAsync(int id, CancellationToken ct = default);
}

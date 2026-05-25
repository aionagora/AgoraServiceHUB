namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface IEstadoFinancieroService
{
    Task<Result<BalanceGeneralDto>> GetBalanceGeneralAsync(DateTime fechaCorte, CancellationToken ct = default);
    Task<Result<EstadoResultadosDto>> GetEstadoResultadosAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<SumasYSaldosDto>> GetSumasYSaldosAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<LibroDiarioDto>> GetLibroDiarioAsync(DateTime desde, DateTime hasta, string? estado = null, CancellationToken ct = default);
    Task<FlujoDEfectivoDto> GetFlujoDEfectivoAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<LibroMayorDto> GetLibroMayorAsync(int empresaId, int cuentaContableId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<RatiosFinancierosDto>> GetRatiosFinancierosAsync(DateTime fechaCorte, CancellationToken ct = default);
}

using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IAuditoriaFEService
{
    /// <summary>
    /// Lista los registros de auditoría FE de la empresa activa, con filtros opcionales.
    /// </summary>
    Task<IReadOnlyList<AuditoriaFEDto>> ListarAsync(
        AuditoriaFEFiltroDto filtro,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista la auditoría de una factura específica (solo si pertenece a la empresa activa).
    /// </summary>
    Task<IReadOnlyList<AuditoriaFEDto>> ListarPorFacturaAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default);
}

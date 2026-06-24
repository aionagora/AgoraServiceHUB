using AgoraHub360.ERP.Domain.Entities.FE;

namespace AgoraHub360.ERP.Domain.Interfaces;

/// <summary>
/// Repositorio de auditoría de Facturación Electrónica (tenant-aware).
/// Solo escritura y consulta histórica — sin actualización ni borrado.
/// </summary>
public interface IAuditoriaFERepository
{
    /// <summary>
    /// Registra un nuevo evento de auditoría (emisión, anulación, consulta).
    /// </summary>
    Task RegistrarAsync(
        AuditoriaFacturacion auditoria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los eventos de auditoría de una factura específica.
    /// </summary>
    Task<IReadOnlyList<AuditoriaFacturacion>> ListarPorFacturaAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los eventos de auditoría de una empresa en un rango de fechas.
    /// </summary>
    Task<IReadOnlyList<AuditoriaFacturacion>> ListarPorEmpresaAsync(
        int empresaId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);
}

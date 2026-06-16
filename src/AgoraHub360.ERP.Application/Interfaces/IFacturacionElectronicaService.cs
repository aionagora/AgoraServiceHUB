using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;
using AgoraHub360.ERP.Application.Common;

namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>
/// Servicio de aplicación para operaciones de Facturación Electrónica.
/// Orquesta la configuración FE activa, el provider correspondiente,
/// la emisión/anulación y el registro de auditoría.
/// </summary>
public interface IFacturacionElectronicaService
{
    /// <summary>
    /// Emite una factura electrónica contra el proveedor activo de la empresa.
    /// </summary>
    Task<Result<EmisionFacturaResultDto>> EmitirAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Anula una factura electrónica contra el proveedor activo de la empresa.
    /// </summary>
    Task<Result<AnulacionFacturaResultDto>> AnularAsync(
        long facturaVentaId,
        string motivoAnulacion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el estado de una factura electrónica contra el proveedor activo.
    /// </summary>
    Task<Result<EstadoFacturaResultDto>> VerificarEstadoAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default);
}

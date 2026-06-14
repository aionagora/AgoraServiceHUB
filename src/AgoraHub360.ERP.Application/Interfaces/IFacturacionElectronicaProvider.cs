using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>
/// Contrato que debe implementar cada proveedor técnico de Facturación Electrónica.
/// La resolución del provider concreto se hace por CodigoProveedor (string),
/// no por enum. Los valores posibles viven en la tabla ProveedorFacturacionElectronica.
/// </summary>
public interface IFacturacionElectronicaProvider
{
    /// <summary>
    /// Código único del proveedor. Debe coincidir con ProveedorFacturacionElectronica.Codigo en BD.
    /// Ej: "CIRRUS", "AGORAFC", "SIAT_DIRECTO".
    /// </summary>
    string CodigoProveedor { get; }

    /// <summary>
    /// Emite una factura electrónica contra el proveedor externo.
    /// </summary>
    Task<EmisionFacturaResultDto> EmitirFacturaAsync(
        ConfiguracionFacturacionElectronica configuracion,
        EmitirFacturaRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Anula una factura electrónica previamente emitida.
    /// </summary>
    Task<AnulacionFacturaResultDto> AnularFacturaAsync(
        ConfiguracionFacturacionElectronica configuracion,
        string cuf,
        string motivoAnulacion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el estado actual de una factura electrónica.
    /// </summary>
    Task<EstadoFacturaResultDto> VerificarEstadoAsync(
        ConfiguracionFacturacionElectronica configuracion,
        string cuf,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene o renueva el CUFD (Código Único de Facturación Diaria) del proveedor.
    /// </summary>
    Task<string> ObtenerCufdAsync(
        ConfiguracionFacturacionElectronica configuracion,
        CancellationToken cancellationToken = default);
}

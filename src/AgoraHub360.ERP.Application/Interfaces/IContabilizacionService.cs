namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>
/// Motor de contabilización automática.
/// Genera asientos contables a partir de plantillas configuradas por tipo de documento.
/// </summary>
public interface IContabilizacionService
{
    /// <summary>
    /// Genera y contabiliza automáticamente un asiento basado en la plantilla del tipo de documento.
    /// </summary>
    /// <param name="tipoDocumento">Tipo de documento: "Recepcion", "Importacion", "Venta", "CostoVenta", "AjusteInventario".</param>
    /// <param name="montos">Diccionario con los montos del documento (Subtotal, Impuesto, Total, GastoAsignado, CostoTotal).</param>
    /// <param name="origenId">Id del documento origen.</param>
    /// <param name="origenReferencia">Referencia legible del origen (ej: REC-000001).</param>
    /// <param name="fecha">Fecha contable del asiento.</param>
    /// <param name="glosaExtra">Información adicional para la glosa (ej: nombre proveedor).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>El asiento generado y contabilizado, o un error si no hay plantilla o falla validación.</returns>
    Task<Result<AsientoContableDto>> ContabilizarDocumentoAsync(
        string tipoDocumento,
        Dictionary<string, decimal> montos,
        long origenId,
        string origenReferencia,
        DateTime fecha,
        string? glosaExtra = null,
        CancellationToken ct = default);
}

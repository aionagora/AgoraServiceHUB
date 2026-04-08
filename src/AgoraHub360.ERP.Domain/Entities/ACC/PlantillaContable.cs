namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Plantilla de contabilización automática.
/// Define qué asiento generar por tipo de documento (Recepción, Importación, Venta, Ajuste, etc.).
/// </summary>
public class PlantillaContable : TenantEntity
{
    public int PlantillaContableId { get; set; }

    /// <summary>Código único de la plantilla (ej: "REC", "IMP", "VTA", "AJU").</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre descriptivo (ej: "Recepción de Compra").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tipo de documento origen que dispara esta plantilla (ej: "Recepcion", "Importacion", "Venta", "AjusteInventario").</summary>
    public string TipoDocumento { get; set; } = string.Empty;

    /// <summary>Descripción / notas de la plantilla.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Glosa por defecto para el asiento generado. Soporta tokens: {Numero}, {Proveedor}, {Fecha}.</summary>
    public string GlosaPlantilla { get; set; } = string.Empty;

    // Navegación
    public ICollection<PlantillaContableLinea> Lineas { get; set; } = new List<PlantillaContableLinea>();
}

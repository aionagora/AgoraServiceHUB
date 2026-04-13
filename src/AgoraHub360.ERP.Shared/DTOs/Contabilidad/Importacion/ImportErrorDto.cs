namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

/// <summary>Detalle de un error detectado durante la importación.</summary>
public class ImportErrorDto
{
    /// <summary>Número de fila en el Excel donde se detectó el error (null si es global).</summary>
    public int? Fila { get; set; }

    /// <summary>Descripción del error.</summary>
    public string Mensaje { get; set; } = string.Empty;
}

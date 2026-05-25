namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

/// <summary>Resultado de la validación previa de un archivo de importación.</summary>
public class ImportValidacionDto
{
    public bool EsValido { get; set; }
    public int TotalFilas { get; set; }
    public int TotalAsientosDetectados { get; set; }
    public List<ImportErrorDto> Errores { get; set; } = new();
}

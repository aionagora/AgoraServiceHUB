namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

/// <summary>Resultado final de la importación masiva de asientos.</summary>
public class ImportResultDto
{
    public int TotalAsientosImportados { get; set; }
    public int TotalLineasImportadas { get; set; }
    public int TotalErrores { get; set; }
    public List<ImportErrorDto> Errores { get; set; } = new();
}

namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

/// <summary>Resultado final de la importación masiva de plan de cuentas.</summary>
public class PlanCuentaImportResultDto
{
    /// <summary>Cantidad de cuentas creadas exitosamente.</summary>
    public int TotalCuentasCreadas { get; set; }

    /// <summary>Cantidad de errores durante la importación.</summary>
    public int TotalErrores { get; set; }

    /// <summary>Detalle de los errores ocurridos durante la importación.</summary>
    public List<ImportErrorDto> Errores { get; set; } = new();
}

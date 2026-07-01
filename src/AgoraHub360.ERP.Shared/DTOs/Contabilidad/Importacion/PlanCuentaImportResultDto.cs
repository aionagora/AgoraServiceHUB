namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

/// <summary>Resultado final de la importación masiva de plan de cuentas.</summary>
public class PlanCuentaImportResultDto
{
    /// <summary>Cantidad de cuentas creadas exitosamente.</summary>
    public int Created { get; set; }

    /// <summary>Alias retrocompatible de Created usado por Infrastructure.</summary>
    public int TotalCuentasCreadas { get => Created; set => Created = value; }

    /// <summary>Cantidad de cuentas actualizadas (no aplica en este flujo).</summary>
    public int Updated { get; set; }

    /// <summary>Cantidad de filas omitidas por errores de validación.</summary>
    public int Skipped { get; set; }

    /// <summary>Número total de errores encontrados durante la importación.</summary>
    public int Errors { get; set; }

    /// <summary>Alias retrocompatible de Errors usado por Infrastructure.</summary>
    public int TotalErrores { get => Errors; set => Errors = value; }

    /// <summary>Detalle de errores de importación por fila.</summary>
    public List<ImportErrorDto> ErrorRows { get; set; } = new();

    /// <summary>Alias retrocompatible de ErrorRows usado por Infrastructure.</summary>
    public List<ImportErrorDto> Errores { get => ErrorRows; set => ErrorRows = value; }

    /// <summary>Mensaje descriptivo del resultado.</summary>
    public string Message { get; set; } = string.Empty;
}

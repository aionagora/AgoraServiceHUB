using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

/// <summary>Fila individual del archivo CSV de plan de cuentas, validada para importación.</summary>
public class PlanCuentaImportRowDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public int Nivel { get; set; } = 1;
    public string? CuentaPadreCodigo { get; set; }
    public bool PermiteMovimientos { get; set; }
    public string? Descripcion { get; set; }
}

/// <summary>Resultado de la validación previa de un archivo CSV de plan de cuentas.</summary>
public class PlanCuentaImportPreviewDto
{
    /// <summary>Nombre del archivo procesado.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Indica si el archivo pasó todas las validaciones estructurales.</summary>
    public bool EsValido { get; set; }

    /// <summary>Total de filas leídas del archivo.</summary>
    public int TotalFilas { get; set; }

    /// <summary>Filas que pasaron las validaciones y están listas para importar.</summary>
    public List<PlanCuentaImportRowDto> CuentasValidas { get; set; } = new();

    /// <summary>Errores de validación detectados (globales y por fila).</summary>
    public List<ImportErrorDto> Errores { get; set; } = new();
}

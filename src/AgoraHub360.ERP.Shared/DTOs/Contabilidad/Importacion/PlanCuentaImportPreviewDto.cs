using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

/// <summary>Fila individual del archivo de plan de cuentas, validada para importación.</summary>
public class PlanCuentaImportRowDto
{
    /// <summary>Número de fila en el archivo original (1-based).</summary>
    public int RowNumber { get; set; }

    /// <summary>Código de la cuenta contable.</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre de la cuenta contable.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tipo de cuenta: Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo.</summary>
    public string TipoCuenta { get; set; } = string.Empty;

    /// <summary>Código de la cuenta padre (opcional).</summary>
    public string? CodigoPadre { get; set; }

    /// <summary>Alias retrocompatible de CodigoPadre usado por Infrastructure.</summary>
    public string? CuentaPadreCodigo { get => CodigoPadre; set => CodigoPadre = value; }

    /// <summary>Nivel jerárquico de la cuenta.</summary>
    public int Nivel { get; set; } = 1;

    /// <summary>Indica si la cuenta permite movimientos contables (detalle).</summary>
    public bool EsMovimiento { get; set; }

    /// <summary>Alias retrocompatible de EsMovimiento usado por Infrastructure.</summary>
    public bool PermiteMovimientos { get => EsMovimiento; set => EsMovimiento = value; }

    /// <summary>Indica si la cuenta está activa.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Indica si la fila pasó todas las validaciones.</summary>
    public bool IsValid { get; set; } = true;

    /// <summary>Lista de errores de validación (vacía si IsValid es true).</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Descripción de la cuenta (usado por Infrastructure).</summary>
    public string? Descripcion { get; set; }

    /// <summary>Nombre del tipo (alias retrocompatible de TipoCuenta usado por Infrastructure).</summary>
    public string Tipo { get => TipoCuenta; set => TipoCuenta = value; }

    /// <summary>Naturaleza de la cuenta (usado por Infrastructure).</summary>
    public string Naturaleza { get; set; } = string.Empty;

    /// <summary>Alias de Naturaleza usado por Infrastructure como NaturalezaStr.</summary>
    public string NaturalezaStr { get => Naturaleza; set => Naturaleza = value; }
}

/// <summary>Resultado de la validación previa de un archivo de plan de cuentas.</summary>
public class PlanCuentaImportPreviewDto
{
    /// <summary>Nombre del archivo procesado.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Total de filas de datos en el archivo (sin incluir encabezado).</summary>
    public int TotalRows { get; set; }

    /// <summary>Alias retrocompatible de TotalRows usado por Infrastructure.</summary>
    public int TotalFilas { get => TotalRows; set => TotalRows = value; }

    /// <summary>Cantidad de filas válidas.</summary>
    public int ValidRows { get; set; }

    /// <summary>Cantidad de filas inválidas.</summary>
    public int InvalidRows { get; set; }

    /// <summary>Indica si se puede proceder con la importación (TotalRows > 0 y sin errores).</summary>
    public bool CanImport { get; set; }

    /// <summary>Alias retrocompatible de CanImport usado por Infrastructure.</summary>
    public bool EsValido { get => CanImport; set => CanImport = value; }

    /// <summary>Lista completa de filas procesadas (válidas e inválidas).</summary>
    public List<PlanCuentaImportRowDto> Rows { get; set; } = new();

    /// <summary>Alias retrocompatible de Rows usado por Infrastructure.</summary>
    public List<PlanCuentaImportRowDto> CuentasValidas { get => Rows; set => Rows = value; }

    /// <summary>Errores de validación (usado por Infrastructure).</summary>
    public List<ImportErrorDto> Errores { get; set; } = new();
}

namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Parámetro de configuración del sistema por empresa (clave-valor).
/// Ejemplos: MonedaBase, ImpuestoDefault, DireccionFiscal.
/// </summary>
public class ParametroSistema : TenantEntity
{
    public int Id { get; set; }

    /// <summary>Clave única del parámetro dentro de la empresa (ej: "MonedaBase").</summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>Valor del parámetro (ej: "BOB").</summary>
    public string Valor { get; set; } = string.Empty;

    /// <summary>Descripción legible del parámetro.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Categoría para agrupar en la UI (ej: "General", "Facturación", "Inventario").</summary>
    public string Categoria { get; set; } = "General";

    /// <summary>Tipo de dato para validación en UI (String, Integer, Decimal, Boolean, Select).</summary>
    public string TipoDato { get; set; } = "String";
}

namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Definición de atributo dinámico por industria.
/// DataType: 1=String, 2=Int, 3=Decimal, 4=Bool, 5=Date, 6=Json, 7=Select
/// </summary>
public class AttributeDefinition : AuditableEntity
{
    public long AttributeId { get; set; }
    public int IndustryId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>1=String, 2=Int, 3=Decimal, 4=Bool, 5=Date, 6=Json, 7=Select</summary>
    public byte DataType { get; set; } = 1;

    public bool IsRequired { get; set; }
    public bool IsSearchable { get; set; }

    /// <summary>Si true, este atributo es un eje de variante (color, talla, sabor…).</summary>
    public bool IsVariantAxis { get; set; }

    public string? ValidationRegex { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? UnitHint { get; set; }

    // Navegación
    public ICollection<AttributeOption> Options { get; set; } = new List<AttributeOption>();
    public ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}

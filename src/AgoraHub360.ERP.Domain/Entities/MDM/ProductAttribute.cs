namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Valor de atributo dinámico asignado a un producto (EAV).
/// Solo uno de los campos Value* estará activo según DataType del atributo.
/// </summary>
public class ProductAttribute : AuditableEntity
{
    public long ProductAttributeId { get; set; }
    public long ProductId { get; set; }
    public long AttributeId { get; set; }

    public string? ValueString { get; set; }
    public decimal? ValueDecimal { get; set; }
    public int? ValueInt { get; set; }
    public bool? ValueBool { get; set; }
    public DateOnly? ValueDate { get; set; }
    public string? ValueJson { get; set; }
    public long? OptionId { get; set; }

    /// <summary>Para versionado de atributos con vigencia.</summary>
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    // Navegación
    public Product Product { get; set; } = null!;
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
    public AttributeOption? Option { get; set; }
}

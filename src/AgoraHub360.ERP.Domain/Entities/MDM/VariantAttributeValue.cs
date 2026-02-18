namespace AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Valor de eje de variante asignado a un ProductVariant.
/// Solo se usan atributos con IsVariantAxis=true.
/// </summary>
public class VariantAttributeValue
{
    public long VariantId { get; set; }
    public long AttributeId { get; set; }

    public long? OptionId { get; set; }
    public string? ValueString { get; set; }

    // Navegación
    public ProductVariant Variant { get; set; } = null!;
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
    public AttributeOption? Option { get; set; }
}

namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Opción de selección para atributos de tipo Select (DataType=7).
/// </summary>
public class AttributeOption : TenantEntity
{
    public long OptionId { get; set; }
    public long AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    // Navegación
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
}

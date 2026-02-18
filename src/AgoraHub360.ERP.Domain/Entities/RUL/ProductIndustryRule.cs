namespace AgoraHub360.ERP.Domain.Entities.RUL;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Regla por industria que define features a activar automáticamente.
/// ConditionJson: condición evaluable. ActionsJson: features a habilitar.
/// Ejemplo ConditionJson: {"ProductKind":1,"IsStockable":true}
/// Ejemplo ActionsJson: {"EnableLot":true,"EnableExpiry":true,"Policy":"FEFO"}
/// </summary>
public class ProductIndustryRule : AuditableEntity
{
    public long RuleId { get; set; }
    public int IndustryId { get; set; }

    public string ConditionJson { get; set; } = "{}";
    public string ActionsJson { get; set; } = "{}";
    public int Priority { get; set; }

    // Navegación
    public Industry Industry { get; set; } = null!;
}

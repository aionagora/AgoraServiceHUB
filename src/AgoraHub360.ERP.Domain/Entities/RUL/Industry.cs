namespace AgoraHub360.ERP.Domain.Entities.RUL;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Industria o segmento de negocio que activa comportamientos específicos de producto.
/// Ejemplos: Farmacia, Ferreteria, Extintores, Alimentos, Produccion.
/// </summary>
public class Industry : AuditableEntity
{
    public int IndustryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductIndustryRule> Rules { get; set; } = new List<ProductIndustryRule>();
}

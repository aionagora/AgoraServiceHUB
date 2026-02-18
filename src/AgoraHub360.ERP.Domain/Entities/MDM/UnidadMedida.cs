namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Unidad de medida para productos (ej: Unidad, Kg, Litro, Caja).
/// </summary>
public class UnidadMedida : TenantEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
}

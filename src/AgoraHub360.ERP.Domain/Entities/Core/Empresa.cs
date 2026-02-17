namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Representa una empresa/tenant en el sistema multi-empresa.
/// </summary>
public class Empresa : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? MonedaBaseId { get; set; }
    public Moneda? MonedaBase { get; set; }
}

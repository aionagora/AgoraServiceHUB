namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Ubicación dentro de un almacén (pasillo, estante, nivel).
/// </summary>
public class UbicacionAlmacen : TenantEntity
{
    public int Id { get; set; }
    public int AlmacenId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

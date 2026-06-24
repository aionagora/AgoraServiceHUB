using AgoraHub360.ERP.Domain.Common;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

public class SiatMetodoPago : TenantEntity
{
    public long Id { get; set; }

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ModoPago { get; set; }
    public bool EsPredeterminado { get; set; }
}

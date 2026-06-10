using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

namespace AgoraHub360.ERP.Domain.Entities.CXC;

public class ClienteCreditoConfiguracion : TenantEntity
{
    public long Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public bool CreditoHabilitado { get; set; } = false;

    public int DiasCredito { get; set; } = 0;

    public decimal LimiteCredito { get; set; } = 0;

    public string Observaciones { get; set; } = string.Empty;
}

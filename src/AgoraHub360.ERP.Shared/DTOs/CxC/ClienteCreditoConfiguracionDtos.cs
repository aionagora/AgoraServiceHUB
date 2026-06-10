namespace AgoraHub360.ERP.Shared.DTOs.CxC;

public class ClienteCreditoConfiguracionDto
{
    public int ClienteId { get; set; }
    public bool CreditoHabilitado { get; set; }
    public int DiasCredito { get; set; }
    public decimal LimiteCredito { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}

public class GuardarClienteCreditoConfiguracionRequestDto
{
    public bool CreditoHabilitado { get; set; }
    public int DiasCredito { get; set; }
    public decimal LimiteCredito { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}

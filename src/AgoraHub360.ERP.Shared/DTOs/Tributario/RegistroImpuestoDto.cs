namespace AgoraHub360.ERP.Shared.DTOs.Tributario;

public class RegistroImpuestoDto
{
    public long RegistroImpuestoId { get; set; }
    public int PeriodoContableId { get; set; }
    public string TipoImpuesto { get; set; } = string.Empty;
    public decimal BaseImponible { get; set; }
    public decimal Tasa { get; set; }
    public decimal MontoCalculado { get; set; }
    public decimal CreditoFiscal { get; set; }
    public decimal DebitoFiscal { get; set; }
    public decimal SaldoAFavor { get; set; }
    public decimal MontoAPagar { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? NumeroCertificado { get; set; }
    public DateTime? FechaDeclaracion { get; set; }
    public long? AsientoContableId { get; set; }
}

public class FormularioSINDto
{
    public string TipoFormulario { get; set; } = string.Empty;
    public int? PeriodoContableId { get; set; }
    public int? Gestion { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal MontoDeterminado { get; set; }
    public decimal CreditoFiscal { get; set; }
    public decimal DebitoFiscal { get; set; }
    public decimal SaldoAFavor { get; set; }
    public decimal MontoAPagar { get; set; }
    public string Moneda { get; set; } = "BOB";
    public Dictionary<string, decimal> CamposAdicionales { get; set; } = new();
}

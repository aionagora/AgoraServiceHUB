namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class TipoComprobanteDto
{
    public int TipoComprobanteId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Prefijo { get; set; } = string.Empty;
    public int Orden { get; set; }
}

public class TipoCambioDto
{
    public int TipoCambioId { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Simbolo { get; set; } = string.Empty;
    public decimal TasaCompra { get; set; }
    public decimal TasaVenta { get; set; }
    public DateTime FechaVigencia { get; set; }
}

public class TipoPagoDto
{
    public int TipoPagoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool RequiereReferencia { get; set; }
    public int Orden { get; set; }
}

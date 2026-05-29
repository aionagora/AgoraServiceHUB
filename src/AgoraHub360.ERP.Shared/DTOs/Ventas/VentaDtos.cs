namespace AgoraHub360.ERP.Shared.DTOs.Ventas;

public class VentaDto
{
    public long Id { get; set; }

    public int SucursalId { get; set; }
    public int? AlmacenId { get; set; }
    public int? ClienteId { get; set; }
    public long? PedidoVentaId { get; set; }

    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }

    public string TipoVenta { get; set; } = string.Empty;
    public string EstadoVenta { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;

    public string? MonedaId { get; set; }
    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal ImpuestoTotal { get; set; }
    public decimal Total { get; set; }

    public string? Observaciones { get; set; }
    public bool FacturaGenerada { get; set; }
    public bool InventarioDescontado { get; set; }

    public VentaFacturacionDatosDto? FacturacionDatos { get; set; }
    public List<VentaDetalleDto> Detalles { get; set; } = new();
    public List<VentaPagoDto> Pagos { get; set; } = new();
}

public class VentaResumenDto
{
    public long Id { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }

    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }

    public int SucursalId { get; set; }
    public string? SucursalNombre { get; set; }

    public string TipoVenta { get; set; } = string.Empty;
    public string EstadoVenta { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;

    public decimal Total { get; set; }
    public bool FacturaGenerada { get; set; }
    public long? PedidoVentaId { get; set; }
}

public class VentaDetalleDto
{
    public long? Id { get; set; }

    public string TipoItemVenta { get; set; } = string.Empty;

    public long? CompanyProductId { get; set; }
    public int? AlmacenId { get; set; }

    public string Descripcion { get; set; } = string.Empty;
    public string? DetalleAdicional { get; set; }

    public decimal Cantidad { get; set; }
    public int? UnidadMedidaId { get; set; }

    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal ImpuestoMonto { get; set; }
    public decimal TotalLinea { get; set; }

    public decimal? CostoUnitario { get; set; }
    public bool DescuentaInventario { get; set; }
}

public class CrearVentaRequestDto
{
    public int SucursalId { get; set; }
    public int? AlmacenId { get; set; }
    public int? ClienteId { get; set; }
    public long? PedidoVentaId { get; set; }

    public string TipoVenta { get; set; } = "Directa";
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

    public string? MonedaId { get; set; }
    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    public List<VentaDetalleDto> Detalles { get; set; } = new();
    public VentaFacturacionDatosDto? FacturacionDatos { get; set; }
    public List<VentaPagoDto>? Pagos { get; set; }

    public string? Observaciones { get; set; }
}

public class ActualizarVentaRequestDto
{
    public int SucursalId { get; set; }
    public int? AlmacenId { get; set; }
    public int? ClienteId { get; set; }

    public DateTime FechaVenta { get; set; }

    public string? MonedaId { get; set; }
    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    public List<VentaDetalleDto> Detalles { get; set; } = new();
    public VentaFacturacionDatosDto? FacturacionDatos { get; set; }

    public string? Observaciones { get; set; }
}

public class ConfirmarVentaRequestDto
{
    public string? Observaciones { get; set; }
}

public class GenerarVentaDesdePedidoRequestDto
{
    public long PedidoVentaId { get; set; }
    public VentaFacturacionDatosDto? FacturacionDatos { get; set; }
    public List<VentaPagoDto>? Pagos { get; set; }
    public string? Observaciones { get; set; }
}

public class VentaFacturacionDatosDto
{
    public bool Facturar { get; set; }
    public bool FacturarAlMismoCliente { get; set; } = true;

    public string? TipoDocumentoIdentidad { get; set; }
    public string? NitFactura { get; set; }
    public string? Complemento { get; set; }
    public string? RazonSocialFactura { get; set; }
    public string? EmailFactura { get; set; }
    public string? TelefonoFactura { get; set; }

    public string EstadoFactura { get; set; } = string.Empty;
    public long? FacturaId { get; set; }
}

public class VentaPagoDto
{
    public DateTime FechaPago { get; set; }

    public string TipoPago { get; set; } = string.Empty;
    public string ModoPago { get; set; } = string.Empty;

    public long? CuentaCajaBancoId { get; set; }

    public decimal Monto { get; set; }

    public string? MonedaId { get; set; }
    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    public string? Referencia { get; set; }

    public string EstadoPago { get; set; } = string.Empty;
}

public class RegistrarPagoVentaRequestDto
{
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    public string TipoPago { get; set; } = string.Empty;
    public string ModoPago { get; set; } = string.Empty;

    public long? CuentaCajaBancoId { get; set; }

    public decimal Monto { get; set; }

    public string? MonedaId { get; set; }
    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    public string? Referencia { get; set; }
    public string EstadoPago { get; set; } = string.Empty;
}

public class AnularVentaRequestDto
{
    public string MotivoAnulacion { get; set; } = string.Empty;
}

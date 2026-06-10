using AgoraHub360.ERP.Shared.DTOs.MDM;
using AgoraHub360.ERP.Shared.DTOs.Sucursal;

namespace AgoraHub360.ERP.Shared.DTOs.Ventas
{
    public class PedidoVentaDto
    {
        public long Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaEntregaEsperada { get; set; }

        public int SucursalId { get; set; }
        public SucursalListadoDto? Sucursal { get; set; }

        public int AlmacenId { get; set; }
        public AlmacenDto? Almacen { get; set; }

        public int VendedorId { get; set; }

        public int ClienteId { get; set; }
        public ClienteDto? Cliente { get; set; }

        public int? ClienteSucursalId { get; set; }
        public ClienteSucursalDto? ClienteSucursal { get; set; }

        public string Prioridad { get; set; } = "Normal";
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }

        public List<PedidoVentaDetalleDto> Detalles { get; set; } = new();
    }

    public class PedidoVentaDetalleDto
    {
        public long Id { get; set; }
        public long CompanyProductId { get; set; }
        public string CompanyProductName { get; set; } = string.Empty;
        public string CompanyProductCode { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }
        public decimal CantidadConfirmada { get; set; }
        public decimal CantidadDespachada { get; set; }

        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CreatePedidoVentaDto
    {
        public DateTime? FechaEntregaEsperada { get; set; }

        /// <summary>Baja, Normal, Alta, Urgente</summary>
        public string Prioridad { get; set; } = "Normal";

        public int SucursalId { get; set; }
        public int AlmacenId { get; set; }

        // Either the Vendedor or the User acts. But we specify the Cliente:
        public int ClienteId { get; set; }
        public int? ClienteSucursalId { get; set; }

        public string? Observaciones { get; set; }

        public List<CreatePedidoVentaDetalleDto> Detalles { get; set; } = new();
    }

    public class CreatePedidoVentaDetalleDto
    {
        public long CompanyProductId { get; set; }
        public decimal CantidadSolicitada { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Impuestos { get; set; }
        public string? Observaciones { get; set; }
    }

    public class UpdatePedidoVentaDto : CreatePedidoVentaDto
    {
        public string Estado { get; set; } = string.Empty;
    }
}

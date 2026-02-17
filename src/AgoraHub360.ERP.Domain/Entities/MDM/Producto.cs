namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Producto del catálogo maestro.
/// </summary>
public class Producto : TenantEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CategoriaProductoId { get; set; }
    public int UnidadMedidaId { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockMinimo { get; set; }
    public string? Sku { get; set; }

    /// <summary>MateriaPrima / ProductoTerminado / Servicio</summary>
    public string TipoProducto { get; set; } = "ProductoTerminado";

    /// <summary>Si false, no se lleva control de stock (ej: Servicios).</summary>
    public bool ControlStock { get; set; } = true;

    /// <summary>Costo base/referencial del producto.</summary>
    public decimal CostoBase { get; set; }
}

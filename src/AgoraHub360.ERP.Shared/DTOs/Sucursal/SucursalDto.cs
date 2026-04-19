namespace AgoraHub360.ERP.Shared.DTOs.Sucursal;

public class SucursalDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? CodigoInterno { get; set; }
    public string? Sigla { get; set; }
    public string? Descripcion { get; set; }
    public string? Pais { get; set; }
    public string? Departamento { get; set; }
    public string? Provincia { get; set; }
    public string? Zona { get; set; }
    public string? Referencia { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string? UrlMapa { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? ResponsableNombre { get; set; }
    public string? ResponsableCargo { get; set; }
    public string? Celular { get; set; }
    public string? WhatsApp { get; set; }
    public string? EmailAlternativo { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool EsCentral { get; set; }
    public bool PermiteVentas { get; set; }
    public bool PermiteCompras { get; set; }
    public bool PermiteInventario { get; set; }
    public bool PermiteDespacho { get; set; }
    public bool PermiteFacturacion { get; set; }
    public bool ManejaAlmacen { get; set; }
    public string? CodigoSucursalFiscal { get; set; }
    public string? PrefijoDocumental { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

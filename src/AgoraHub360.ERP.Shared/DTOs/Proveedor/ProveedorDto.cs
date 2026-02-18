namespace AgoraHub360.ERP.Shared.DTOs.Proveedor;

public class ProveedorDto
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string NIT { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string TipoProveedor { get; set; } = "Local";
    public bool Activo { get; set; }
    public int EmpresaId { get; set; }
}

namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProveedorDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? NombreContacto { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

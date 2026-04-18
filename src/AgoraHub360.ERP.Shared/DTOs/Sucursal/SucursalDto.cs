namespace AgoraHub360.ERP.Shared.DTOs.Sucursal;

public class SucursalDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool EsCentral { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

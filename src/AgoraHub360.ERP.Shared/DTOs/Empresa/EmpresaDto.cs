namespace AgoraHub360.ERP.Shared.DTOs.Empresa;

public class EmpresaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? MonedaBaseId { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

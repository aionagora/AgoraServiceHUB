namespace AgoraHub360.ERP.Shared.DTOs.Sucursal;

public class SucursalListadoDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Ciudad { get; set; }
    public bool EsCentral { get; set; }
    public bool Activo { get; set; }
}

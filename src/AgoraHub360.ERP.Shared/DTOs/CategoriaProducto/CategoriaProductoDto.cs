namespace AgoraHub360.ERP.Shared.DTOs.CategoriaProducto;

public class CategoriaProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public int EmpresaId { get; set; }
}

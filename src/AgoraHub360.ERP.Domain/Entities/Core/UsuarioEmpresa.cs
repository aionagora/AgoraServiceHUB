namespace AgoraHub360.ERP.Domain.Entities.Core;

/// <summary>
/// Relación muchos-a-muchos entre Usuario y Empresa, con rol asignado.
/// </summary>
public class UsuarioEmpresa
{
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public string Rol { get; set; } = "Viewer";
}

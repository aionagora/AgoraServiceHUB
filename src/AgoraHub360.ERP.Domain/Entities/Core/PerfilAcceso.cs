namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Perfil de acceso parametrizable por empresa para usuarios internos o externos.
/// </summary>
public class PerfilAcceso : TenantEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string TipoUsuario { get; set; } = "Ambos";

    public ICollection<PerfilPermiso> Permisos { get; set; } = new List<PerfilPermiso>();
    public ICollection<UsuarioPerfil> Usuarios { get; set; } = new List<UsuarioPerfil>();
    public ICollection<ContactoUsuarioAcceso> ContactosExternos { get; set; } = new List<ContactoUsuarioAcceso>();
}

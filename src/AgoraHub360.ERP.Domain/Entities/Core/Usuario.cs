namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Representa un usuario del sistema con soporte multi-empresa.
/// </summary>
public class Usuario : AuditableEntity
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int? EmpresaActivaId { get; set; }
    public Empresa? EmpresaActiva { get; set; }
    public ICollection<UsuarioEmpresa> Empresas { get; set; } = new List<UsuarioEmpresa>();
    public ICollection<UsuarioSucursalAcceso> SucursalesAcceso { get; set; } = new List<UsuarioSucursalAcceso>();
    public ICollection<ContactoUsuarioAcceso> ContactosAccesos { get; set; } = new List<ContactoUsuarioAcceso>();
    public ICollection<UsuarioPerfil> Perfiles { get; set; } = new List<UsuarioPerfil>();
}

namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Rol del sistema RBAC. Tabla catálogo que define los roles disponibles.
/// El campo UsuarioEmpresa.Rol referencia Rol.Nombre.
/// </summary>
public class Rol : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

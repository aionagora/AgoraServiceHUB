namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Asignación de perfiles a usuarios por empresa.
/// </summary>
public class UsuarioPerfil : TenantEntity
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int PerfilAccesoId { get; set; }
    public PerfilAcceso? PerfilAcceso { get; set; }

    public DateTime? VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
}

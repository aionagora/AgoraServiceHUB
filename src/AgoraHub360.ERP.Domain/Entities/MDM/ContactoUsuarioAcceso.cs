namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

/// <summary>
/// Configuración de acceso externo para un contacto de cliente.
/// </summary>
public class ContactoUsuarioAcceso : TenantEntity
{
    public int Id { get; set; }

    public int ContactoId { get; set; }
    public Contacto? Contacto { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int? PerfilAccesoId { get; set; }
    public PerfilAcceso? PerfilAcceso { get; set; }

    public bool AccesoWeb { get; set; }
    public bool AccesoMovil { get; set; }
}

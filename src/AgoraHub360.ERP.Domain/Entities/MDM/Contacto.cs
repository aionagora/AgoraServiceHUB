namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Contacto de cliente para comunicación comercial y habilitación opcional de acceso externo.
/// </summary>
public class Contacto : TenantEntity
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? ClienteSucursalId { get; set; }
    public ClienteSucursal? ClienteSucursal { get; set; }

    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public bool EsPrincipal { get; set; }

    public ICollection<ContactoUsuarioAcceso> AccesosUsuario { get; set; } = new List<ContactoUsuarioAcceso>();
}

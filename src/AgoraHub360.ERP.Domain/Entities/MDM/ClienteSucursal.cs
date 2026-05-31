namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Sucursal o dirección operativa de un cliente.
/// Diseñada para soportar pedidos, ventas y logística.
/// </summary>
public class ClienteSucursal : TenantEntity
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? AliasComercial { get; set; }

    public int? PaisId { get; set; }
    public int? DepartamentoId { get; set; }
    public int? ProvinciaId { get; set; }
    public int? CiudadId { get; set; }
    public int? ZonaId { get; set; }

    public string? Direccion { get; set; }
    public string? Referencia { get; set; }
    public string? UrlMapa { get; set; }

    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    public int? ContactoPrincipalId { get; set; }
    public string? ResponsableNombre { get; set; }
    public string? ResponsableCargo { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }

    public bool EsPrincipal { get; set; }
    public bool RecibePedidos { get; set; } = true;
    public bool RecibeFacturacion { get; set; } = true;
    public bool EsPuntoEntrega { get; set; } = true;

    public string? Observaciones { get; set; }

    public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
    public ICollection<ClientePerfilFiscal> PerfilesFiscales { get; set; } = new List<ClientePerfilFiscal>();
}

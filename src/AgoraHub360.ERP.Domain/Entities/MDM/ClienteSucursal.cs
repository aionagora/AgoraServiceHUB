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

    public int? PaisId { get; set; }
    public int? DepartamentoId { get; set; }
    public int? ProvinciaId { get; set; }
    public int? CiudadId { get; set; }
    public int? ZonaId { get; set; }

    public string? Direccion { get; set; }
    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    public int? ContactoPrincipalId { get; set; }

    public bool EsPrincipal { get; set; }

    public string? Observaciones { get; set; }
}

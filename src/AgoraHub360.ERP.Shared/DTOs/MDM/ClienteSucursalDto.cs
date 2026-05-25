namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClienteSucursalDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int EmpresaId { get; set; }

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
    public bool Activo { get; set; }
    public bool RecibePedidos { get; set; }
    public bool RecibeFacturacion { get; set; }
    public bool EsPuntoEntrega { get; set; }
    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }
}

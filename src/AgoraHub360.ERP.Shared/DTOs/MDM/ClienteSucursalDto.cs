namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClienteSucursalDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int EmpresaId { get; set; }

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
    public bool Activo { get; set; }
    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }
}

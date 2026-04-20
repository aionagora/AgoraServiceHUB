namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class ContactoDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int EmpresaId { get; set; }
    public int? ClienteSucursalId { get; set; }

    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public bool EsPrincipal { get; set; }
    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}

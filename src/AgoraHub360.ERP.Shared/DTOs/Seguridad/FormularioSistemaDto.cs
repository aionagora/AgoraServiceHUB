namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class FormularioSistemaDto
{
    public int Id { get; set; }
    public int ModuloSistemaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public bool VisibleEnMenu { get; set; }
    public List<AccionSistemaDto> Acciones { get; set; } = new();
}

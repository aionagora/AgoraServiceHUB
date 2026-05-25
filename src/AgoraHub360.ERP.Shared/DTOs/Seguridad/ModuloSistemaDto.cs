namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class ModuloSistemaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public List<FormularioSistemaDto> Formularios { get; set; } = new();
}

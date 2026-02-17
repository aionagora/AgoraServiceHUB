namespace AgoraHub360.ERP.Shared.DTOs.Parametro;

public class ParametroSistemaDto
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = "General";
    public string TipoDato { get; set; } = "String";
    public int EmpresaId { get; set; }
    public bool Activo { get; set; }
}

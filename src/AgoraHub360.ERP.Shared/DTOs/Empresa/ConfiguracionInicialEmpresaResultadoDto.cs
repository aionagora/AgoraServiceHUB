namespace AgoraHub360.ERP.Shared.DTOs.Empresa;

public class ConfiguracionInicialEmpresaResultadoDto
{
    public int ParametrosCreados { get; set; }
    public int ParametrosExistentes { get; set; }
    public int NumeracionesCreadas { get; set; }
    public int NumeracionesExistentes { get; set; }
    public int CatalogosCreados { get; set; }
    public int CatalogosExistentes { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

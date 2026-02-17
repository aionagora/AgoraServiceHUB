namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class MdmSearchFilterDto
{
    public string? Search { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}

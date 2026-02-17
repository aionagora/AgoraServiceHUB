namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class UnidadMedidaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// DTO público del catálogo de ambientes de Facturación Electrónica.
/// </summary>
public class AmbienteFEDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsProduccion { get; set; }
    public bool Activo { get; set; }
}

namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// DTO público del catálogo de proveedores de Facturación Electrónica.
/// </summary>
public class ProveedorFEDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool RequierePosToken { get; set; }
    public bool SoportaAnulacion { get; set; }
    public bool SoportaConsultaEstado { get; set; }
    public bool SoportaModoOffline { get; set; }
    public bool Activo { get; set; }
}

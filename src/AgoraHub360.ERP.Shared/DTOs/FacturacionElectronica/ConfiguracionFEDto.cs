namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// DTO de configuración FE para salida (nunca expone secretos).
/// TieneClientSecret y TienePosToken indican si hay valores cifrados almacenados.
/// </summary>
public class ConfiguracionFEDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string NombreConfiguracion { get; set; } = string.Empty;

    public int ProveedorFacturacionElectronicaId { get; set; }
    public string ProveedorCodigo { get; set; } = string.Empty;
    public string ProveedorNombre { get; set; } = string.Empty;

    public int AmbienteFacturacionElectronicaId { get; set; }
    public string AmbienteCodigo { get; set; } = string.Empty;
    public string AmbienteNombre { get; set; } = string.Empty;
    public bool EsProduccion { get; set; }

    public string ClientId { get; set; } = string.Empty;
    /// <summary>Indica si hay un ClientSecret cifrado almacenado (no se expone el valor).</summary>
    public bool TieneClientSecret { get; set; }

    public string TokenUrl { get; set; } = string.Empty;
    public string ApiManagementUrl { get; set; } = string.Empty;
    public string ApiBillingUrl { get; set; } = string.Empty;

    /// <summary>Indica si hay un PosToken cifrado almacenado (no se expone el valor).</summary>
    public bool TienePosToken { get; set; }

    public string? SucursalFiscal { get; set; }
    public string? PuntoVentaFiscal { get; set; }
    public string ActivityCode { get; set; } = string.Empty;
    public string NitEmisor { get; set; } = string.Empty;
    public int TimeoutSegundos { get; set; }
    public bool EsConfiguracionActiva { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; }
}

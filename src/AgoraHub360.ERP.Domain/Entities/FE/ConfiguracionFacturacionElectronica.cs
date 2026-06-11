using AgoraHub360.ERP.Domain.Common;

namespace AgoraHub360.ERP.Domain.Entities.FE;

/// <summary>
/// Configuración de Facturación Electrónica por empresa (tenant-aware).
/// Cada empresa puede tener múltiples configuraciones, pero solo una activa a la vez.
/// Almacena credenciales cifradas, URLs, ActivityCode, NIT emisor, proveedor y ambiente.
/// </summary>
public class ConfiguracionFacturacionElectronica : TenantEntity
{
    public int Id { get; set; }

    /// <summary>Nombre descriptivo de esta configuración. Ej: "Configuración Principal", "Sucursal Central".</summary>
    public string NombreConfiguracion { get; set; } = string.Empty;

    // ── Proveedor y Ambiente (catálogos globales) ──────────────────────────

    /// <summary>FK al proveedor de facturación electrónica seleccionado.</summary>
    public int ProveedorFacturacionElectronicaId { get; set; }

    /// <summary>Proveedor de facturación electrónica.</summary>
    public ProveedorFacturacionElectronica? ProveedorFacturacionElectronica { get; set; }

    /// <summary>FK al ambiente de facturación electrónica seleccionado.</summary>
    public int AmbienteFacturacionElectronicaId { get; set; }

    /// <summary>Ambiente de facturación electrónica.</summary>
    public AmbienteFacturacionElectronica? AmbienteFacturacionElectronica { get; set; }

    // ── Credenciales (siempre cifradas) ────────────────────────────────────

    /// <summary>Client ID para autenticación con el proveedor. No es secreto.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Client Secret cifrado. Nunca en texto plano.</summary>
    public string ClientSecretEncrypted { get; set; } = string.Empty;

    /// <summary>URL del servicio de autenticación/token del proveedor.</summary>
    public string TokenUrl { get; set; } = string.Empty;

    /// <summary>URL base de la API de gestión/management del proveedor.</summary>
    public string ApiManagementUrl { get; set; } = string.Empty;

    /// <summary>URL base de la API de facturación/billing del proveedor.</summary>
    public string ApiBillingUrl { get; set; } = string.Empty;

    /// <summary>PosToken cifrado (si aplica según el proveedor). Nunca en texto plano.</summary>
    public string PosTokenEncrypted { get; set; } = string.Empty;

    // ── Configuración fiscal ───────────────────────────────────────────────

    /// <summary>Código de sucursal fiscal asignado por el proveedor/SIAT.</summary>
    public string? SucursalFiscal { get; set; }

    /// <summary>Código de punto de venta fiscal asignado por el proveedor/SIAT.</summary>
    public string? PuntoVentaFiscal { get; set; }

    /// <summary>Código de actividad económica (ActivityCode) para la facturación.</summary>
    public string ActivityCode { get; set; } = string.Empty;

    /// <summary>NIT del emisor (empresa).</summary>
    public string NitEmisor { get; set; } = string.Empty;

    // ── Configuración técnica ──────────────────────────────────────────────

    /// <summary>Timeout en segundos para llamadas HTTP al proveedor.</summary>
    public int TimeoutSegundos { get; set; } = 10;

    /// <summary>Indica si esta es la configuración activa de la empresa. Solo una puede ser activa.</summary>
    public bool EsConfiguracionActiva { get; set; } = true;

    /// <summary>Observaciones opcionales.</summary>
    public string? Observaciones { get; set; }
}

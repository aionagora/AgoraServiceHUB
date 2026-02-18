namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Proveedor del sistema.
/// </summary>
public class Proveedor : TenantEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? NombreContacto { get; set; }

    /// <summary>Local / Internacional</summary>
    public string TipoProveedor { get; set; } = "Local";

    /// <summary>País del proveedor (relevante si es Internacional).</summary>
    public string? Pais { get; set; }

    /// <summary>Condición de pago (ej: Contado, 30 días, 60 días).</summary>
    public string? CondicionPago { get; set; }
}

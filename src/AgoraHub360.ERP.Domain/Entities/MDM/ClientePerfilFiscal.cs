namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

public class ClientePerfilFiscal : TenantEntity
{
    public long Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? ClienteSucursalId { get; set; }
    public ClienteSucursal? ClienteSucursal { get; set; }

    public string Alias { get; set; } = string.Empty;
    public string TipoDocumentoIdentidad { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string RazonSocial { get; set; } = string.Empty;

    public string? TipoPersona { get; set; }
    public string? TipoPerfilFiscal { get; set; }

    public string? EmailFactura { get; set; }
    public string? TelefonoFactura { get; set; }

    public bool RequiereEmail { get; set; } = false;
    public bool EsPredeterminado { get; set; }
    public bool ValidadoFacturacion { get; set; } = false;

    public string? CodigoClienteApi { get; set; }
    public string? CodigoExternoFacturacion { get; set; }
    public string? Observaciones { get; set; }
}

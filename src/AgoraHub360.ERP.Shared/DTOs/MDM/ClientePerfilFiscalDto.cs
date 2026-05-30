namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClientePerfilFiscalDto
{
    public long Id { get; set; }
    public int ClienteId { get; set; }
    public int? ClienteSucursalId { get; set; }

    public string Alias { get; set; } = string.Empty;
    public string TipoDocumentoIdentidad { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string RazonSocial { get; set; } = string.Empty;

    public string? TipoPersona { get; set; }
    public string? TipoPerfilFiscal { get; set; }

    public string? EmailFactura { get; set; }
    public string? TelefonoFactura { get; set; }

    public bool RequiereEmail { get; set; }
    public bool EsPredeterminado { get; set; }
    public bool ValidadoFacturacion { get; set; }

    public string? CodigoClienteApi { get; set; }
    public string? CodigoExternoFacturacion { get; set; }
    public string? Observaciones { get; set; }

    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

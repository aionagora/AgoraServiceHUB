namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Multi-identificador del producto: código de barras, QR, SKU externo por proveedor/cliente/canal.
/// EmpresaId puede ser null si el código es global.
/// CodeType: 1=SKU, 2=Barra, 3=QR, 4=ExternoProveedor, 5=ExternoCliente, 6=Canal, 7=InternoAlterno
/// </summary>
public class ProductCode : AuditableEntity
{
    public long ProductCodeId { get; set; }
    public int? EmpresaId { get; set; }
    public long ProductId { get; set; }

    public byte CodeType { get; set; }
    public string Valor { get; set; } = string.Empty;

    public long? ProviderId { get; set; }
    public long? CustomerId { get; set; }
    public int? ChannelId { get; set; }

    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public Product Product { get; set; } = null!;
}

namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Representa una empresa/tenant en el sistema multi-empresa.
/// Contiene configuración global de producto, costeo e industria.
/// </summary>
public class Empresa : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? MonedaBaseId { get; set; }
    public Moneda? MonedaBase { get; set; }

    // ?? Configuración de productos ??????????????????????????????????????????

    /// <summary>Industria principal: 1=Retail, 2=Alimentos, 3=Farmacia, 4=Ferretería, 5=Textil, 6=Tecnología, 7=General</summary>
    public byte IndustriaId { get; set; } = 7;

    /// <summary>Método de costeo por defecto. 1=Promedio, 2=FIFO, 3=LIFO, 4=Estándar</summary>
    public byte MetodoCosteoDefault { get; set; } = 1;

    /// <summary>Si true, permite gestionar variantes de producto (talla, color, etc.)</summary>
    public bool PermiteVariantes { get; set; } = true;

    /// <summary>Si true, permite lotes y números de serie en productos</summary>
    public bool PermiteLotes { get; set; }

    /// <summary>Si true, gestiona productos como servicio (no stockable)</summary>
    public bool PermiteServicios { get; set; } = true;

    /// <summary>Si true, genera SKU automático al crear producto empresa</summary>
    public bool AutoGeneraSku { get; set; } = true;

    /// <summary>Prefijo para SKU autogenerado. Ej: "PRD-"</summary>
    public string? PrefijoSku { get; set; }

    public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();
}

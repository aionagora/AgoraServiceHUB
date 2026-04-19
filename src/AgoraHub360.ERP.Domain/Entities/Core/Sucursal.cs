namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Representa una sucursal, tienda o ubicación de la Empresa.
/// </summary>
public class Sucursal : TenantEntity
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Codigo { get; set; }

    // -- Nuevos campos de Identificación --
    public string? CodigoInterno { get; set; }
    public string? Sigla { get; set; }
    public string? Descripcion { get; set; }

    // -- Nuevos campos de Ubicación (adicionales a Ciudad y Dirección) --
    public Guid? PaisId { get; set; }
    public Guid? DepartamentoId { get; set; }
    public Guid? ProvinciaId { get; set; }
    public Guid? CiudadId { get; set; }
    public Guid? ZonaId { get; set; }

    public string? Pais { get; set; }
    public string? Departamento { get; set; }
    public string? Provincia { get; set; }
    public string? Zona { get; set; }
    public string? Referencia { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string? UrlMapa { get; set; }

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    // -- Nuevos campos de Contacto (adicionales a Telefono y Email) --
    public string? ResponsableNombre { get; set; }
    public string? ResponsableCargo { get; set; }
    public string? Celular { get; set; }
    public string? WhatsApp { get; set; }
    public string? EmailAlternativo { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public bool EsCentral { get; set; }

    // -- Configuración Operativa --
    public bool PermiteVentas { get; set; } = true;
    public bool PermiteCompras { get; set; } = true;
    public bool PermiteInventario { get; set; } = true;
    public bool PermiteDespacho { get; set; } = true;
    public bool PermiteFacturacion { get; set; } = true;
    public bool ManejaAlmacen { get; set; } = true;

    // -- Configuración Adicional --
    public string? CodigoSucursalFiscal { get; set; }
    public string? PrefijoDocumental { get; set; }
    public string? Observaciones { get; set; }

    // Relación
    public Empresa? Empresa { get; set; }
}

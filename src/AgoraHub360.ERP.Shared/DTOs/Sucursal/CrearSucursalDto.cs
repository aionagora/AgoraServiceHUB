namespace AgoraHub360.ERP.Shared.DTOs.Sucursal;

using System.ComponentModel.DataAnnotations;

public class CrearSucursalDto
{
    public int EmpresaId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "El código no puede exceder 50 caracteres.")]
    public string? Codigo { get; set; }

    [MaxLength(50, ErrorMessage = "El código interno no puede exceder 50 caracteres.")]
    public string? CodigoInterno { get; set; }

    [MaxLength(20, ErrorMessage = "La sigla no puede exceder 20 caracteres.")]
    public string? Sigla { get; set; }

    [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Descripcion { get; set; }

    public Guid? PaisId { get; set; }
    public Guid? DepartamentoId { get; set; }
    public Guid? ProvinciaId { get; set; }
    public Guid? CiudadId { get; set; }
    public Guid? ZonaId { get; set; }

    [MaxLength(100, ErrorMessage = "El país no puede exceder 100 caracteres.")]
    public string? Pais { get; set; }

    [MaxLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
    public string? Departamento { get; set; }

    [MaxLength(100, ErrorMessage = "La provincia no puede exceder 100 caracteres.")]
    public string? Provincia { get; set; }

    [MaxLength(100, ErrorMessage = "La zona no puede exceder 100 caracteres.")]
    public string? Zona { get; set; }

    [MaxLength(500, ErrorMessage = "La referencia no puede exceder 500 caracteres.")]
    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    [MaxLength(1000, ErrorMessage = "La URL del mapa no puede exceder 1000 caracteres.")]
    public string? UrlMapa { get; set; }

    [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres.")]
    public string? Direccion { get; set; }

    [MaxLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres.")]
    public string? Ciudad { get; set; }

    [MaxLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres.")]
    public string? Telefono { get; set; }

    [MaxLength(200, ErrorMessage = "El nombre del responsable no puede exceder 200 caracteres.")]
    public string? ResponsableNombre { get; set; }

    [MaxLength(100, ErrorMessage = "El cargo del responsable no puede exceder 100 caracteres.")]
    public string? ResponsableCargo { get; set; }

    [MaxLength(50, ErrorMessage = "El celular no puede exceder 50 caracteres.")]
    public string? Celular { get; set; }

    [MaxLength(50, ErrorMessage = "El WhatsApp no puede exceder 50 caracteres.")]
    public string? WhatsApp { get; set; }

    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "El email alternativo no puede exceder 200 caracteres.")]
    [EmailAddress(ErrorMessage = "El formato del email alternativo no es válido.")]
    public string? EmailAlternativo { get; set; }

    public bool EsCentral { get; set; }

    public bool PermiteVentas { get; set; } = true;

    public bool PermiteCompras { get; set; } = true;

    public bool PermiteInventario { get; set; } = true;

    public bool PermiteDespacho { get; set; } = true;

    public bool PermiteFacturacion { get; set; } = true;

    public bool ManejaAlmacen { get; set; } = true;

    [MaxLength(50, ErrorMessage = "El código fiscal no puede exceder 50 caracteres.")]
    public string? CodigoSucursalFiscal { get; set; }

    [MaxLength(20, ErrorMessage = "El prefijo documental no puede exceder 20 caracteres.")]
    public string? PrefijoDocumental { get; set; }

    [MaxLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres.")]
    public string? Observaciones { get; set; }

    public bool Activo { get; set; } = true;
}

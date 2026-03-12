namespace AgoraHub360.ERP.Shared.DTOs.Logistica;

/// <summary>DTO de lectura para Hoja de Ruta.</summary>
public class HojaRutaDto
{
    public long HojaRutaId { get; set; }
    public int EmpresaId { get; set; }
    public string NumeroHojaRuta { get; set; } = string.Empty;
    public string TipoOP { get; set; } = string.Empty;
    public string SubTipo { get; set; } = string.Empty;
    public int AlmacenOrigenId { get; set; }
    public string? AlmacenOrigenNombre { get; set; }
    public int? AlmacenDestinoId { get; set; }
    public string? AlmacenDestinoNombre { get; set; }
    public string? ProveedorCliente { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? ContactoCliente { get; set; }
    public string ResponsableUsuario { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public DateTime FechaDocumento { get; set; }
    public DateTime? ETA { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string SubEstado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public bool Activo { get; set; }
    public long? OrdenPedidoId { get; set; }
    public List<HojaRutaHistorialDto> Historial { get; set; } = new();
}

/// <summary>DTO de lectura para historial de cambios de estado.</summary>
public class HojaRutaHistorialDto
{
    public long HojaRutaHistorialId { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string SubEstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string SubEstadoNuevo { get; set; } = string.Empty;
    public DateTime FechaCambio { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

/// <summary>DTO para crear una Hoja de Ruta.</summary>
public class CreateHojaRutaDto
{
    public string TipoOP { get; set; } = "INTERNA";
    public string SubTipo { get; set; } = string.Empty;
    public int AlmacenOrigenId { get; set; }
    public int? AlmacenDestinoId { get; set; }
    public string? ProveedorCliente { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? ContactoCliente { get; set; }
    public DateTime FechaDocumento { get; set; }
    public DateTime? ETA { get; set; }
    public string? Observaciones { get; set; }
    public long? OrdenPedidoId { get; set; }
}

/// <summary>DTO para actualizar una Hoja de Ruta en BORRADOR.</summary>
public class UpdateHojaRutaDto
{
    public string TipoOP { get; set; } = "INTERNA";
    public string SubTipo { get; set; } = string.Empty;
    public int AlmacenOrigenId { get; set; }
    public int? AlmacenDestinoId { get; set; }
    public string? ProveedorCliente { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? ContactoCliente { get; set; }
    public DateTime FechaDocumento { get; set; }
    public DateTime? ETA { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>DTO para cambiar estado/subestado de una Hoja de Ruta.</summary>
public class CambiarEstadoHojaRutaDto
{
    public string NuevoEstado { get; set; } = string.Empty;
    public string NuevoSubEstado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

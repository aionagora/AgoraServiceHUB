namespace AgoraHub360.ERP.Shared.DTOs.AuditLog;

/// <summary>
/// Filtros para consulta de logs de auditoría.
/// </summary>
public class AuditLogFilterDto
{
    public string? Entidad { get; set; }
    public string? EntidadId { get; set; }
    public string? Accion { get; set; }
    public string? Usuario { get; set; }
    public int? EmpresaId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 50;
}

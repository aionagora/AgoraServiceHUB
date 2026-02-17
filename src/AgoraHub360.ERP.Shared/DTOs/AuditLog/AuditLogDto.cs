namespace AgoraHub360.ERP.Shared.DTOs.AuditLog;

public class AuditLogDto
{
    public long Id { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string? CamposModificados { get; set; }
    public int? EmpresaId { get; set; }
    public string? Usuario { get; set; }
    public DateTime FechaHora { get; set; }
}

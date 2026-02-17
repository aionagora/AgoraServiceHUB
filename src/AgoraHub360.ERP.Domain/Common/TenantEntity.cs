namespace AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Entidad base tenant-aware.
/// Todas las entidades que pertenecen a una empresa específica heredan de esta clase.
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public int EmpresaId { get; set; }
}

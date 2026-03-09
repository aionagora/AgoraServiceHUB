namespace AgoraHub360.ERP.Domain.Entities.CST;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

/// <summary>
/// Centro de costo para analítica contable.
/// Permite estructura jerárquica mediante auto-referencia (ParentId).
/// </summary>
public class CentroCosto : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>Código único dentro de la empresa (ej: CC-VEN-01).</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre descriptivo del centro de costo.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción opcional.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Centro de costo padre (jerarquía). Null = nivel raíz.</summary>
    public int? ParentId { get; set; }

    /// <summary>Empresa a la que pertenece el centro de costo.</summary>
    public int EmpresaId { get; set; }

    public virtual CentroCosto? Parent { get; set; }
    public virtual ICollection<CentroCosto> Children { get; set; } = new List<CentroCosto>();
    public virtual Empresa? Empresa { get; set; }
}

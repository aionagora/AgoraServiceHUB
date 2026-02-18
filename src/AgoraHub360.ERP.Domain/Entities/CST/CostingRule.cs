namespace AgoraHub360.ERP.Domain.Entities.CST;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Regla de método de costeo por empresa y tipo de producto.
/// DefaultMethod: 1=Promedio, 2=FIFO, 3=Estándar
/// </summary>
public class CostingRule : AuditableEntity
{
    public int EmpresaId { get; set; }
    public byte ProductKind { get; set; }

    /// <summary>1=Promedio, 2=FIFO, 3=Estándar</summary>
    public byte DefaultMethod { get; set; } = 1;
}

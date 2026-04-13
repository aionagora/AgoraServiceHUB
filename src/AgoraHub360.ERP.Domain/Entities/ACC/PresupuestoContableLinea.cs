namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.CST;

public class PresupuestoContableLinea : AuditableEntity
{
    public int PresupuestoContableLineaId { get; private set; }
    public int PresupuestoContableId { get; private set; }
    public int CuentaContableId { get; private set; }
    public int? CentroCostoId { get; private set; }
    public int Mes { get; private set; } // 1-12
    public decimal MontoPresupuestado { get; private set; }

    public CuentaContable CuentaContable { get; private set; } = null!;
    public CentroCosto? CentroCosto { get; private set; }
    public PresupuestoContable PresupuestoContable { get; private set; } = null!;

    protected PresupuestoContableLinea() { }

    public PresupuestoContableLinea(int cuentaContableId, int? centroCostoId, int mes, decimal montoPresupuestado)
    {
        CuentaContableId = cuentaContableId;
        CentroCostoId = centroCostoId;
        Mes = mes;
        MontoPresupuestado = montoPresupuestado;
    }
}

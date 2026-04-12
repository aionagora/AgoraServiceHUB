namespace AgoraHub360.ERP.Domain.Entities.ACT;

using System;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;

public class DepreciacionMensual : AuditableEntity
{
    public long DepreciacionMensualId { get; private set; }
    public long ActivoFijoId { get; private set; }
    public int PeriodoContableId { get; private set; }
    public decimal Monto { get; private set; }
    public DateTime Fecha { get; private set; }
    public long? AsientoContableId { get; private set; }

    public ActivoFijo ActivoFijo { get; private set; } = null!;
    public PeriodoContable PeriodoContable { get; private set; } = null!;
    public AsientoContable? AsientoContable { get; private set; }

    protected DepreciacionMensual() { }

    public DepreciacionMensual(int periodoId, decimal monto, DateTime fecha)
    {
        PeriodoContableId = periodoId;
        Monto = monto;
        Fecha = fecha;
    }

    public void VincularAsiento(long asientoContableId)
    {
        AsientoContableId = asientoContableId;
    }
}

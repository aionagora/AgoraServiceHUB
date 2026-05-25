namespace AgoraHub360.ERP.Domain.Entities.BNC;

using System;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;

public class ConciliacionBancaria : TenantEntity
{
    public int ConciliacionBancariaId { get; private set; }
    public int CuentaContableId { get; private set; }
    public int PeriodoContableId { get; private set; }
    public decimal SaldoExtracto { get; private set; }
    public decimal SaldoContable { get; private set; }
    public decimal Diferencia => SaldoExtracto - SaldoContable;
    public bool EstaConciliado => Math.Abs(Diferencia) <= 0.01m;
    public string Estado { get; private set; } = "EnProceso"; // EnProceso|Conciliado|Aprobado

    public CuentaContable CuentaContable { get; private set; } = null!;
    public PeriodoContable PeriodoContable { get; private set; } = null!;

    protected ConciliacionBancaria() { }

    public ConciliacionBancaria(
        int empresaId,
        int cuentaContableId,
        int periodoContableId,
        decimal saldoExtracto,
        decimal saldoContable)
    {
        EmpresaId = empresaId;
        CuentaContableId = cuentaContableId;
        PeriodoContableId = periodoContableId;
        SaldoExtracto = saldoExtracto;
        SaldoContable = saldoContable;
        Estado = "EnProceso";
    }

    public void ActualizarSaldos(decimal saldoExtracto, decimal saldoContable)
    {
        SaldoExtracto = saldoExtracto;
        SaldoContable = saldoContable;
        Estado = EstaConciliado ? "Conciliado" : "EnProceso";
    }

    public Result Aprobar()
    {
        if (!EstaConciliado)
            return Result.Failure("No se puede aprobar una conciliación con diferencias.");

        Estado = "Aprobado";
        return Result.Success();
    }
}

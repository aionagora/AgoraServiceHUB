namespace AgoraHub360.ERP.Domain.Entities.BNC;

using System;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Exceptions;

public class ExtractoBancario : TenantEntity
{
    public long ExtractoBancarioId { get; private set; }
    public int CuentaContableId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Monto { get; private set; } // positivo=ingreso, negativo=egreso
    public string? NumeroReferencia { get; private set; }
    public bool Conciliado { get; private set; }
    public long? AsientoContableLineaId { get; private set; }

    public CuentaContable CuentaContable { get; private set; } = null!;
    public AsientoContableLinea? AsientoContableLinea { get; private set; }

    protected ExtractoBancario() { }

    public ExtractoBancario(
        int empresaId,
        int cuentaContableId,
        DateTime fecha,
        string descripcion,
        decimal monto,
        string? numeroReferencia = null)
    {
        EmpresaId = empresaId;
        CuentaContableId = cuentaContableId;
        Fecha = fecha;
        Descripcion = descripcion;
        Monto = monto;
        NumeroReferencia = numeroReferencia;
        Conciliado = false;
    }

    public void Conciliar(long asientoLineaId)
    {
        if (Conciliado)
            throw new DomainException("El movimiento ya está conciliado.");

        Conciliado = true;
        AsientoContableLineaId = asientoLineaId;
    }

    public void Desconciliar()
    {
        Conciliado = false;
        AsientoContableLineaId = null;
    }
}

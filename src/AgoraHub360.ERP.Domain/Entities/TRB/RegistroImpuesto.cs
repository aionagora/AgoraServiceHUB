namespace AgoraHub360.ERP.Domain.Entities.TRB;

using System;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Exceptions;

public class RegistroImpuesto : TenantEntity
{
    public long RegistroImpuestoId { get; private set; }
    public int PeriodoContableId { get; private set; }
    public string TipoImpuesto { get; private set; } = string.Empty; // IVA | IT | IUE | RC-IVA
    public decimal BaseImponible { get; private set; }
    public decimal Tasa { get; private set; }
    public decimal MontoCalculado { get; private set; }
    public decimal CreditoFiscal { get; private set; }      // Solo IVA
    public decimal DebitoFiscal { get; private set; }       // Solo IVA
    public decimal SaldoAFavor { get; private set; }        // Arrastre IVA
    public decimal MontoAPagar { get; private set; }
    public string Estado { get; private set; } = "Calculado"; // Calculado | Declarado | Pagado
    public string? NumeroCertificado { get; private set; }
    public DateTime? FechaDeclaracion { get; private set; }
    public long? AsientoContableId { get; private set; }

    public PeriodoContable PeriodoContable { get; private set; } = null!;
    public AsientoContable? AsientoContable { get; private set; }

    protected RegistroImpuesto() { }

    public void MarcarDeclarado(string numeroCertificado, DateTime fechaDeclaracion)
    {
        if (Estado != "Calculado")
            throw new DomainException("Solo se puede declarar un impuesto en estado Calculado.");

        NumeroCertificado = numeroCertificado;
        FechaDeclaracion = fechaDeclaracion;
        Estado = "Declarado";
    }

    public void MarcarPagado(long asientoId)
    {
        if (Estado != "Declarado")
            throw new DomainException("El impuesto debe estar Declarado antes de marcarse como Pagado.");

        AsientoContableId = asientoId;
        Estado = "Pagado";
    }
}

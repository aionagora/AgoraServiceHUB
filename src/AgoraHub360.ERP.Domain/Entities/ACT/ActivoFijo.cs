namespace AgoraHub360.ERP.Domain.Entities.ACT;

using System;
using System.Collections.Generic;
using System.Linq;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;

public class ActivoFijo : TenantEntity
{
    public long ActivoFijoId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string CategoriaActivo { get; private set; } = string.Empty; // Edificio|Vehiculo|Maquinaria|EquipoComp|Mobiliario
    
    public int CuentaContableId { get; private set; }
    public CuentaContable CuentaContable { get; private set; } = null!;
    
    public int CuentaDepreciacionId { get; private set; }
    public CuentaContable CuentaDepreciacion { get; private set; } = null!;
    
    public int CuentaGastoDepreciacionId { get; private set; }
    public CuentaContable CuentaGastoDepreciacion { get; private set; } = null!;
    
    public DateTime FechaAdquisicion { get; private set; }
    public decimal CostoAdquisicion { get; private set; }
    public decimal ValorResidual { get; private set; }
    public decimal TasaAnualDS24051 { get; private set; }
    public int VidaUtilAnios { get; private set; }
    public decimal DepreciacionAcumulada { get; private set; }
    public decimal ValorEnLibros => CostoAdquisicion - DepreciacionAcumulada - ValorResidual;
    public bool DepreciacionCompleta { get; private set; }
    public string Estado { get; private set; } = "Activo"; // Activo|Dado de Baja|Vendido

    public ICollection<DepreciacionMensual> Depreciaciones { get; private set; } = new List<DepreciacionMensual>();

    protected ActivoFijo() { }

    public decimal CalcularCuotaMensual()
    {
        if (DepreciacionCompleta) return 0m;

        var baseDepreciable = CostoAdquisicion - ValorResidual;
        return Math.Round(baseDepreciable * TasaAnualDS24051 / 12m, 2);
    }

    public Result AplicarDepreciacionMensual(int periodoId, DateTime fecha)
    {
        if (DepreciacionCompleta)
            return Result.Failure("El activo ya está totalmente depreciado.");

        if (Estado != "Activo")
            return Result.Failure($"No se puede depreciar un activo en estado {Estado}.");

        if (Depreciaciones.Any(d => d.PeriodoContableId == periodoId))
            return Result.Failure("Ya existe depreciación para este período.");

        var cuota = CalcularCuotaMensual();
        var baseDepreciable = CostoAdquisicion - ValorResidual;
        var nuevaAcumulada = DepreciacionAcumulada + cuota;

        if (nuevaAcumulada >= baseDepreciable)
        {
            cuota = baseDepreciable - DepreciacionAcumulada;
            nuevaAcumulada = baseDepreciable;
            DepreciacionCompleta = true;
        }

        DepreciacionAcumulada = nuevaAcumulada;
        Depreciaciones.Add(new DepreciacionMensual(periodoId, cuota, fecha));

        return Result.Success();
    }

    public Result DarDeBaja(string motivo)
    {
        if (Estado == "Dado de Baja")
            return Result.Failure("El activo ya está dado de baja.");

        // TODO: En un escenario real se podría guardar el motivo en un campo de auditoría o historial
        Estado = "Dado de Baja";
        return Result.Success();
    }
}

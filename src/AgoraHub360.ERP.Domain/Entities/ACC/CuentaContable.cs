namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Enums;

public enum ClasificacionFlujoEfectivo
{
    NoAplica = 0,
    Operacional = 1,
    Inversion = 2,
    Financiacion = 3
}

/// <summary>
/// Cuenta del Plan de Cuentas contable.
/// Estructura jerárquica N-nivel con código estructurado (ej: 1.1.3.01).
/// </summary>
public class CuentaContable : TenantEntity
{
    public int CuentaContableId { get; set; }

    /// <summary>Código estructurado de la cuenta (ej: "1", "1.1", "1.1.3", "1.1.3.01").</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre descriptivo de la cuenta.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tipo de cuenta: Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo.</summary>
    public TipoCuenta Tipo { get; set; }

    /// <summary>Naturaleza del saldo: Deudora o Acreedora.</summary>
    public NaturalezaCuenta Naturaleza { get; set; }

    /// <summary>Nivel jerárquico (1=grupo, 2=subgrupo, 3=cuenta, 4=subcuenta…).</summary>
    public int Nivel { get; set; } = 1;

    /// <summary>Id de la cuenta padre (null para cuentas de nivel 1).</summary>
    public int? CuentaPadreId { get; set; }
    public CuentaContable? CuentaPadre { get; set; }

    /// <summary>True si acepta movimientos (es hoja). False si es solo agrupadora.</summary>
    public bool PermiteMovimientos { get; set; }

    /// <summary>Descripción o notas adicionales.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Saldo actual de la cuenta (se actualiza con cada asiento).</summary>
    public decimal SaldoActual { get; set; }

    /// <summary>Clasificación para el estado del flujo de efectivo.</summary>
    public ClasificacionFlujoEfectivo ClasificacionFlujo { get; private set; } = ClasificacionFlujoEfectivo.NoAplica;

    // Navegación
    public ICollection<CuentaContable> SubCuentas { get; set; } = new List<CuentaContable>();

    /// <summary>Actualiza la clasificación para el flujo de efectivo.</summary>
    public void ActualizarClasificacionFlujo(ClasificacionFlujoEfectivo clasificacion)
    {
        ClasificacionFlujo = clasificacion;
    }
}

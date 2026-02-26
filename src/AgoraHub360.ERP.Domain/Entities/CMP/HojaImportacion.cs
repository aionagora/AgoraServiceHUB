namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Hoja de Importación (Landed Cost Sheet).
/// Agrupa gastos de importación y los distribuye sobre las líneas de una OC recepcionada
/// para calcular el costo real unitario (Landed Cost).
/// </summary>
public class HojaImportacion : TenantEntity
{
    public long HojaImportacionId { get; set; }

    /// <summary>Número único (ej: IMP-000001).</summary>
    public string Numero { get; set; } = string.Empty;

    public long OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }

    public DateTime Fecha { get; set; }

    /// <summary>Referencia DUI/DIM u otro documento aduanero.</summary>
    public string? ReferenciaAduanera { get; set; }

    public string? Observaciones { get; set; }

    /// <summary>Método de distribución de gastos: 1=Peso, 2=Volumen, 3=Valor, 4=Unidades.</summary>
    public byte MetodoDistribucion { get; set; } = 3;

    /// <summary>Suma total de gastos de importación.</summary>
    public decimal TotalGastos { get; set; }

    /// <summary>True si ya se liquidó y actualizó los costos de inventario.</summary>
    public bool Liquidada { get; set; }

    // Navegación
    public ICollection<GastoImportacion> Gastos { get; set; } = new List<GastoImportacion>();
    public ICollection<ImportacionLinea> Lineas { get; set; } = new List<ImportacionLinea>();
}

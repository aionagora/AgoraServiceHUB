namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

// Nota: Estas clases están encapsuladas en una directiva preprocesador 
// para cumplir ambas reglas solicitadas:
// 1. NO eliminar todavía las clases de AsientoContableDto.cs
// 2. NO romper compilación (CS0101 / CS0229 - Ambigüedad de clases con mismo nombre y namespace)
// En el próximo paso, al eliminar las originales, se puede remover la directiva.

#if PREPARACION_REFACTOR
public class TipoComprobanteDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Prefijo { get; set; } = string.Empty;
}

public class TipoCambioDto
{
    public int Id { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public decimal ValorCompra { get; set; }
    public decimal ValorVenta { get; set; }
    public DateTime Fecha { get; set; }
}

public class TipoPagoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class CentroCostoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int? PadreId { get; set; }
    public bool Activo { get; set; }
}
#endif

namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Serie de numeración de documentos por empresa.
/// Cada tipo de documento tiene su propio correlativo.
/// </summary>
public class NumeracionDocumento : TenantEntity
{
    public int Id { get; set; }

    /// <summary>Código del tipo de documento (ej: "OC", "REC", "FAC", "PED", "NC").</summary>
    public string TipoDocumento { get; set; } = string.Empty;

    /// <summary>Nombre legible del tipo (ej: "Orden de Compra").</summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Prefijo de la serie (ej: "OC-", "FAC-").</summary>
    public string Prefijo { get; set; } = string.Empty;

    /// <summary>Siguiente número a asignar.</summary>
    public int SiguienteNumero { get; set; } = 1;

    /// <summary>Cantidad de dígitos con ceros a la izquierda (ej: 6 → 000001).</summary>
    public int Digitos { get; set; } = 6;

    /// <summary>Genera el siguiente número formateado (ej: "OC-000001") y avanza el contador.</summary>
    public string GenerarSiguiente()
    {
        var numero = $"{Prefijo}{SiguienteNumero.ToString().PadLeft(Digitos, '0')}";
        SiguienteNumero++;
        return numero;
    }
}

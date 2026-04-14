namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

using System;

public class CierreContableDto
{
    public int Gestion { get; set; }
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;

    // Resultados del cierre anual (nulos en el cierre básico)
    public decimal? UtilidadNeta { get; set; }
    public decimal? MontoIUE { get; set; }
    public decimal? ReservaLegal { get; set; }
    public long? AsientoIUEId { get; set; }
    public long? AsientoReservaLegalId { get; set; }
    public long? AsientoResultadoId { get; set; }
    public long? AsientoTransferenciaId { get; set; }
    public bool NuevaGestionAbierta { get; set; }
}

public class EjecutarCierreDto
{
    public int Gestion { get; set; }
    public DateTime FechaCierre { get; set; }
}

/// <summary>DTO para ejecutar el cierre anual completo con IUE, reserva legal y apertura opcional.</summary>
public class EjecutarCierreAnualDto
{
    public int Gestion { get; set; }
    public DateTime FechaCierre { get; set; }

    /// <summary>Si true, se creará el asiento de apertura para la gestión siguiente.</summary>
    public bool AbrirNuevaGestion { get; set; }

    /// <summary>Observaciones opcionales registradas en el cierre.</summary>
    public string? Observaciones { get; set; }
}

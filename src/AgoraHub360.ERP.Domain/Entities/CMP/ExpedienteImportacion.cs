namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Enums;

/// <summary>
/// Expediente de Importación / Embarque.
/// Agrupa 1..N Órdenes de Compra en un mismo embarque internacional.
/// Representa los pasos [6] Embarque ? [7] Despacho Aduanero ? [8] Levante.
/// Flujo: Borrador ? EnTransito ? Arribado ? EnAduana
///        ? (ObservacionAduana ? subsanar ? EnAduana) ? Liberado ? Cerrado.
/// </summary>
public class ExpedienteImportacion : TenantEntity
{
    public long ExpedienteImportacionId { get; set; }

    /// <summary>Número único (ej: EXP-000001).</summary>
    public string Numero { get; set; } = string.Empty;

    public EstadoDocumento Estado { get; set; } = EstadoDocumento.Borrador;

    // ?? Datos de embarque ??????????????????????????????????????????????????

    /// <summary>Incoterm negociado (FOB, CIF, EXW, DDP, etc.).</summary>
    public string? Incoterm { get; set; }

    /// <summary>Modalidad de transporte: Marítimo, Aéreo, Terrestre, Multimodal.</summary>
    public string? ModalidadTransporte { get; set; }

    /// <summary>País de origen de la carga.</summary>
    public string? PaisOrigen { get; set; }

    /// <summary>Puerto/aeropuerto de origen.</summary>
    public string? PuertoOrigen { get; set; }

    /// <summary>Puerto/aeropuerto de destino.</summary>
    public string? PuertoDestino { get; set; }

    /// <summary>Forwarder/agente de carga responsable.</summary>
    public string? Forwarder { get; set; }

    /// <summary>Compañía aseguradora.</summary>
    public string? Aseguradora { get; set; }

    /// <summary>Número de póliza de seguro.</summary>
    public string? NumeroPólizaSeguro { get; set; }

    // ?? Tracking ???????????????????????????????????????????????????????????

    /// <summary>Número de Bill of Lading (BL) o Air Waybill (AWB).</summary>
    public string? NumeroBLAWB { get; set; }

    /// <summary>Estimated Time of Departure – fecha estimada de salida.</summary>
    public DateTime? ETD { get; set; }

    /// <summary>Actual Time of Departure – fecha real de salida.</summary>
    public DateTime? ATD { get; set; }

    /// <summary>Estimated Time of Arrival – fecha estimada de arribo.</summary>
    public DateTime? ETA { get; set; }

    /// <summary>Actual Time of Arrival – fecha real de arribo.</summary>
    public DateTime? ATA { get; set; }

    // ?? Despacho Aduanero ??????????????????????????????????????????????????

    /// <summary>Número DUI/DIM u otro documento aduanero oficial.</summary>
    public string? NumeroDUIDIM { get; set; }

    /// <summary>Despachante de aduana asignado.</summary>
    public string? Despachante { get; set; }

    /// <summary>Fecha de presentación de documentos en aduana.</summary>
    public DateTime? FechaPresentacionAduana { get; set; }

    /// <summary>Monto total de tributos/aranceles pagados.</summary>
    public decimal? TotalTributos { get; set; }

    /// <summary>True si hubo observación o aforo aduanero.</summary>
    public bool TuvoObservacionAduana { get; set; }

    /// <summary>Descripción de la observación/aforo.</summary>
    public string? DetalleObservacionAduana { get; set; }

    /// <summary>Fecha en que se obtuvo el levante/liberación.</summary>
    public DateTime? FechaLevante { get; set; }

    public string? Observaciones { get; set; }

    // Navegación

    /// <summary>Órdenes de Compra vinculadas a este expediente.</summary>
    public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();

    /// <summary>Hitos de tracking registrados.</summary>
    public ICollection<HitoExpediente> Hitos { get; set; } = new List<HitoExpediente>();

    /// <summary>Hojas de importación (landed cost) asociadas al expediente.</summary>
    public ICollection<HojaImportacion> HojasImportacion { get; set; } = new List<HojaImportacion>();
}

namespace AgoraHub360.ERP.Domain.Entities.Core;

/// <summary>
/// Registro de auditoría: captura cada cambio (INSERT, UPDATE, DELETE)
/// realizado sobre entidades del sistema.
/// No hereda de AuditableEntity para evitar recursión en el interceptor.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    /// <summary>Nombre de la entidad afectada (ej: "Empresa", "Usuario").</summary>
    public string Entidad { get; set; } = string.Empty;

    /// <summary>Clave primaria de la entidad afectada (como string).</summary>
    public string EntidadId { get; set; } = string.Empty;

    /// <summary>Tipo de operación: Insert, Update, Delete.</summary>
    public string Accion { get; set; } = string.Empty;

    /// <summary>JSON con los valores anteriores (sólo en Update/Delete).</summary>
    public string? ValoresAnteriores { get; set; }

    /// <summary>JSON con los valores nuevos (sólo en Insert/Update).</summary>
    public string? ValoresNuevos { get; set; }

    /// <summary>Lista de propiedades modificadas separadas por coma (sólo en Update).</summary>
    public string? CamposModificados { get; set; }

    /// <summary>EmpresaId del contexto al momento del cambio.</summary>
    public int? EmpresaId { get; set; }

    /// <summary>Usuario que realizó el cambio.</summary>
    public string? Usuario { get; set; }

    /// <summary>Fecha y hora UTC del cambio.</summary>
    public DateTime FechaHora { get; set; }
}

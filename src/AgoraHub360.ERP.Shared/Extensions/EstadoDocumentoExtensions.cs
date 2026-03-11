namespace AgoraHub360.ERP.Shared.Extensions;

public static class EstadoDocumentoExtensions
{
    /// <summary>
    /// Traduce el valor string del estado (tal como viene en los DTOs) a un nombre legible en español.
    /// </summary>
    public static string ToNombre(this string estado) => estado switch
    {
        "Borrador"                => "Borrador",
        "Confirmado"              => "Confirmado",
        "EnRevision"              => "En Revisión",
        "AbastecidoConStock"      => "Abastecido con Stock",
        "PendienteAprobacion"     => "Pte. Aprobación",
        "Aprobado"                => "Aprobado",
        "EnviadaProveedor"        => "Enviada a Proveedor",
        "EnNegociacion"           => "En Negociación",
        "ConfirmadaProveedor"     => "Confirmada Proveedor",
        "PagoProgramado"          => "Pago Programado",
        "EnTransito"              => "En Tránsito",
        "Arribado"                => "Arribado",
        "EnAduana"                => "En Aduana",
        "ObservacionAduana"       => "Obs. Aduana",
        "Liberado"                => "Liberado",
        "RecepcionParcial"        => "Recepción Parcial",
        "RecepcionConDiferencias" => "Recepción c/Diferencias",
        "Cerrado"                 => "Cerrado",
        "Anulado"                 => "Anulado",
        "Rechazado"               => "Rechazado",
        _                         => estado
    };

    /// <summary>
    /// Devuelve las clases CSS de Bootstrap para el badge según el estado del documento.
    /// </summary>
    public static string ToBadgeClass(this string estado) => estado switch
    {
        "Borrador"                => "bg-secondary",
        "Confirmado"              => "bg-primary",
        "EnRevision"              => "bg-info text-dark",
        "AbastecidoConStock"      => "bg-success",
        "PendienteAprobacion"     => "bg-warning text-dark",
        "Aprobado"                => "bg-success",
        "EnviadaProveedor"        => "bg-primary",
        "EnNegociacion"           => "bg-warning text-dark",
        "ConfirmadaProveedor"     => "bg-success",
        "PagoProgramado"          => "bg-info text-dark",
        "EnTransito"              => "bg-primary",
        "Arribado"                => "bg-info text-dark",
        "EnAduana"                => "bg-warning text-dark",
        "ObservacionAduana"       => "bg-danger",
        "Liberado"                => "bg-success",
        "RecepcionParcial"        => "bg-info text-dark",
        "RecepcionConDiferencias" => "bg-warning text-dark",
        "Cerrado"                 => "bg-dark",
        "Anulado"                 => "bg-danger",
        "Rechazado"               => "bg-danger",
        _                         => "bg-secondary"
    };

    /// <summary>
    /// Devuelve las clases CSS de Bootstrap para el badge de urgencia.
    /// </summary>
    public static string ToUrgenciaBadgeClass(this string urgencia) => urgencia switch
    {
        "Urgente" => "bg-warning text-dark",
        "Critico" => "bg-danger",
        _         => "bg-light text-dark border"
    };
}

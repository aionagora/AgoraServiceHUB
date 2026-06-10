namespace AgoraHub360.ERP.Web.Helpers;

public static class BadgeCssHelper
{
    public static string GetEstadoVentaBadge(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return "badge bg-secondary";

        return estado.Trim().ToLowerInvariant() switch
        {
            "borrador" => "badge bg-secondary",
            "confirmada" => "badge bg-primary",
            "facturada" => "badge bg-success",
            "anulada" or "anulado" => "badge bg-danger",
            _ => "badge bg-secondary"
        };
    }

    public static string GetEstadoFacturaBadge(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return "badge bg-secondary";

        return estado.Trim().ToLowerInvariant() switch
        {
            "borrador" => "badge bg-secondary",
            "generada" => "badge bg-success",
            "anulada" or "anulado" => "badge bg-danger",
            _ => "badge bg-secondary"
        };
    }

    public static string GetEstadoPagoBadge(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return "badge bg-secondary";

        return estado.Trim().ToLowerInvariant() switch
        {
            "pendiente" => "badge bg-warning text-dark",
            "parcial" => "badge bg-info text-dark",
            "pagada" or "pagado" => "badge bg-success",
            "anulada" or "anulado" => "badge bg-danger",
            _ => "badge bg-secondary"
        };
    }

    public static string GetEstadoCxCBadge(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return "badge bg-secondary";

        return estado.Trim().ToLowerInvariant() switch
        {
            "pendiente" => "badge bg-warning text-dark",
            "parcial" => "badge bg-info text-dark",
            "pagada" or "pagado" => "badge bg-success",
            "vencida" or "vencido" => "badge bg-danger",
            "anulada" or "anulado" => "badge bg-secondary",
            _ => "badge bg-secondary"
        };
    }

    public static string GetTipoPagoBadge(string? tipoPago)
    {
        if (string.IsNullOrWhiteSpace(tipoPago)) return "badge bg-secondary";

        return tipoPago.Trim().ToLowerInvariant() switch
        {
            "efectivo" => "badge bg-success",
            "qr" => "badge bg-primary",
            "transferencia" => "badge bg-info text-dark",
            "deposito" or "depósito" => "badge bg-secondary",
            "tarjeta" => "badge bg-dark",
            "cheque" => "badge bg-warning text-dark",
            "credito" or "crédito" => "badge bg-danger",
            _ => "badge bg-secondary"
        };
    }
}

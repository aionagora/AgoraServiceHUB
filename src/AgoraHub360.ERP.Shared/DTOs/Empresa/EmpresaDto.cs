namespace AgoraHub360.ERP.Shared.DTOs.Empresa;

public class EmpresaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? MonedaBaseId { get; set; }
    /// <summary>Símbolo de la moneda base ("Bs.", "$", "€", etc.). Resuelto desde la entidad Moneda.</summary>
    public string? SimboloMoneda { get; set; }
    /// <summary>Decimales a mostrar para montos en la moneda base. Default 2.</summary>
    public int DecimalesMoneda { get; set; } = 2;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

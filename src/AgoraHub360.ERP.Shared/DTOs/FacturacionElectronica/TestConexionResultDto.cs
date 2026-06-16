namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// Resultado de una prueba de conexión contra el proveedor FE.
/// No expone secretos.
/// </summary>
public class TestConexionResultDto
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? ProveedorCodigo { get; set; }
    public string? AmbienteCodigo { get; set; }
    public long TiempoRespuestaMs { get; set; }
    public string? DetalleTecnico { get; set; }
}

namespace AgoraHub360.ERP.Shared.DTOs.Numeracion;

public class NumeracionDocumentoDto
{
    public int Id { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Prefijo { get; set; } = string.Empty;
    public int SiguienteNumero { get; set; }
    public int Digitos { get; set; }
    public int EmpresaId { get; set; }
    public bool Activo { get; set; }

    /// <summary>Vista previa del siguiente número formateado.</summary>
    public string PreviewSiguiente => $"{Prefijo}{SiguienteNumero.ToString().PadLeft(Digitos, '0')}";
}

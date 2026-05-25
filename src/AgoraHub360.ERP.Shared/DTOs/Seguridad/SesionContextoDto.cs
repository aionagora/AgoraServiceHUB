namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class SesionContextoDto
{
    public int UsuarioId { get; set; }
    public int EmpresaId { get; set; }

    public List<PerfilUsuarioSesionDto> Perfiles { get; set; } = new();
    public List<ModuloSistemaDto> ModulosPermitidos { get; set; } = new();
    public List<UsuarioSucursalAccesoDto> SucursalesPermitidas { get; set; } = new();

    public int? SucursalPredeterminadaId { get; set; }
}

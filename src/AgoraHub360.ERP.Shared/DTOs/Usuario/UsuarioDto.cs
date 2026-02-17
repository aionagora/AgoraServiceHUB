namespace AgoraHub360.ERP.Shared.DTOs.Usuario;

public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int? EmpresaActivaId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<UsuarioEmpresaRolDto> EmpresasAsignadas { get; set; } = new();
}

public class UsuarioEmpresaRolDto
{
    public int EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

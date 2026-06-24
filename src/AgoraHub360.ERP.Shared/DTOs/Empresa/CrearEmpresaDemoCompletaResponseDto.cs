namespace AgoraHub360.ERP.Shared.DTOs.Empresa;

public class CrearEmpresaDemoCompletaResponseDto
{
    public string CodigoDemo { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;

    public bool EmpresaCreada { get; set; }
    public bool EmpresaExistente { get; set; }

    public List<string> Usuarios { get; set; } = new();
    public List<string> Sucursales { get; set; } = new();
    public List<string> Almacenes { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();

    public int UsuariosCreados { get; set; }
    public int UsuariosExistentes { get; set; }

    public int SucursalesCreadas { get; set; }
    public int SucursalesExistentes { get; set; }

    public int AlmacenesCreados { get; set; }
    public int AlmacenesExistentes { get; set; }

    public int ClientesCreados { get; set; }
    public int ClientesExistentes { get; set; }

    public int ProveedoresCreados { get; set; }
    public int ProveedoresExistentes { get; set; }

    public int ProductosCreados { get; set; }
    public int ProductosExistentes { get; set; }

    public int NumeracionesCreadas { get; set; }
    public int NumeracionesExistentes { get; set; }

    public bool ResetSolicitado { get; set; }
    public string? Advertencia { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

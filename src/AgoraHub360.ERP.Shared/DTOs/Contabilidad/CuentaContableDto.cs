namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>DTO de lectura para Cuenta Contable.</summary>
public class CuentaContableDto
{
    public int CuentaContableId { get; set; }
    public int EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public int? CuentaPadreId { get; set; }
    public string? CuentaPadreCodigo { get; set; }
    public string? CuentaPadreNombre { get; set; }
    public bool PermiteMovimientos { get; set; }
    public string? Descripcion { get; set; }
    public decimal SaldoActual { get; set; }
    public bool Activo { get; set; }
    public List<CuentaContableDto> SubCuentas { get; set; } = new();
}

/// <summary>DTO para crear una cuenta contable.</summary>
public class CreateCuentaContableDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public byte Tipo { get; set; } = 1;
    public byte Naturaleza { get; set; } = 1;
    public int Nivel { get; set; } = 1;
    public int? CuentaPadreId { get; set; }
    public bool PermiteMovimientos { get; set; }
    public string? Descripcion { get; set; }
}

/// <summary>DTO para actualizar una cuenta contable.</summary>
public class UpdateCuentaContableDto
{
    public string Nombre { get; set; } = string.Empty;
    public byte Tipo { get; set; }
    public byte Naturaleza { get; set; }
    public bool PermiteMovimientos { get; set; }
    public string? Descripcion { get; set; }
}

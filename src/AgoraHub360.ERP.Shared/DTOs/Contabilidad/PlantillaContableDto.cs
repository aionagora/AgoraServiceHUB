namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>DTO de lectura para Plantilla Contable.</summary>
public class PlantillaContableDto
{
    public int PlantillaContableId { get; set; }
    public int EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string GlosaPlantilla { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public List<PlantillaContableLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO de lectura para línea de plantilla.</summary>
public class PlantillaContableLineaDto
{
    public int PlantillaContableLineaId { get; set; }
    public int NumeroLinea { get; set; }
    public int CuentaContableId { get; set; }
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public string TipoMovimiento { get; set; } = string.Empty;
    public string CampoMonto { get; set; } = string.Empty;
    public decimal Factor { get; set; }
    public string? Glosa { get; set; }
}

/// <summary>DTO para crear una plantilla contable con líneas.</summary>
public class CreatePlantillaContableDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string GlosaPlantilla { get; set; } = string.Empty;
    public List<CreatePlantillaLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO para actualizar una plantilla (reemplaza líneas).</summary>
public class UpdatePlantillaContableDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string GlosaPlantilla { get; set; } = string.Empty;
    public List<CreatePlantillaLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO para crear una línea de plantilla.</summary>
public class CreatePlantillaLineaDto
{
    public int CuentaContableId { get; set; }
    public string TipoMovimiento { get; set; } = "Debe";
    public string CampoMonto { get; set; } = "Total";
    public decimal Factor { get; set; } = 1m;
    public string? Glosa { get; set; }
}

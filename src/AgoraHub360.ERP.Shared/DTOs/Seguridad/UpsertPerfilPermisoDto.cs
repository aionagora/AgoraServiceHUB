namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

using System.ComponentModel.DataAnnotations;

public class UpsertPerfilPermisoDto
{
    [Range(1, int.MaxValue)]
    public int PerfilAccesoId { get; set; }

    [Range(1, int.MaxValue)]
    public int FormularioSistemaId { get; set; }

    public int? AccionSistemaId { get; set; }
    public bool Permitido { get; set; } = true;
}

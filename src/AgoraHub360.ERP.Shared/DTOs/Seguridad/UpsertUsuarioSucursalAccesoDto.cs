namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

using System.ComponentModel.DataAnnotations;

public class UpsertUsuarioSucursalAccesoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Sucursal inválida.")]
    public int SucursalId { get; set; }

    public bool EsPredeterminada { get; set; }
    public bool PuedeConsultar { get; set; } = true;
    public bool PuedeOperar { get; set; } = true;
    public bool Activo { get; set; } = true;
}

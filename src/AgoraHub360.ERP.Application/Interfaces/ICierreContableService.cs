namespace AgoraHub360.ERP.Application.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface ICierreContableService
{
    Task<CierreContableDto> EjecutarCierreAsync(int empresaId, EjecutarCierreDto dto, CancellationToken ct);
    Task<bool> ExisteCierreAsync(int empresaId, int gestion, CancellationToken ct);
    Task<CierreContableDto?> ObtenerCierreAsync(int empresaId, int gestion, CancellationToken ct);
    Task EjecutarAperturaAsync(int empresaId, int gestionNueva, CancellationToken ct);
}

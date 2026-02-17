namespace AgoraHub360.ERP.Shared.DTOs;

/// <summary>
/// Resultado paginado genérico reutilizable por todos los módulos.
/// </summary>
public class PaginatedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas => TamanoPagina > 0
        ? (int)Math.Ceiling(TotalItems / (double)TamanoPagina)
        : 0;
}

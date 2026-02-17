namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Shared.DTOs.AuditLog;

public class AuditLogDtoTests
{
    [Fact]
    public void PaginatedResult_TotalPaginas_CalculatedCorrectly()
    {
        var result = new PaginatedResultDto<AuditLogDto>
        {
            TotalItems = 120,
            TamanoPagina = 50,
            Pagina = 1
        };

        Assert.Equal(3, result.TotalPaginas); // ceil(120/50) = 3
    }

    [Fact]
    public void PaginatedResult_TotalPaginas_ZeroSize_ReturnsZero()
    {
        var result = new PaginatedResultDto<AuditLogDto>
        {
            TotalItems = 10,
            TamanoPagina = 0
        };

        Assert.Equal(0, result.TotalPaginas);
    }

    [Fact]
    public void PaginatedResult_EmptyItems_DefaultsCorrectly()
    {
        var result = new PaginatedResultDto<AuditLogDto>();

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.TotalPaginas);
    }

    [Fact]
    public void AuditLogFilterDto_DefaultValues()
    {
        var filter = new AuditLogFilterDto();

        Assert.Null(filter.Entidad);
        Assert.Null(filter.EntidadId);
        Assert.Null(filter.Accion);
        Assert.Null(filter.Usuario);
        Assert.Null(filter.EmpresaId);
        Assert.Null(filter.FechaDesde);
        Assert.Null(filter.FechaHasta);
        Assert.Equal(1, filter.Pagina);
        Assert.Equal(50, filter.TamanoPagina);
    }

    [Fact]
    public void AuditLogDto_MapsAllProperties()
    {
        var now = DateTime.UtcNow;
        var dto = new AuditLogDto
        {
            Id = 1,
            Entidad = "Empresa",
            EntidadId = "5",
            Accion = "Update",
            ValoresAnteriores = "{}",
            ValoresNuevos = "{}",
            CamposModificados = "Nombre",
            EmpresaId = 1,
            Usuario = "admin",
            FechaHora = now
        };

        Assert.Equal(1, dto.Id);
        Assert.Equal("Empresa", dto.Entidad);
        Assert.Equal("5", dto.EntidadId);
        Assert.Equal("Update", dto.Accion);
        Assert.Equal("Nombre", dto.CamposModificados);
        Assert.Equal(1, dto.EmpresaId);
        Assert.Equal("admin", dto.Usuario);
        Assert.Equal(now, dto.FechaHora);
    }
}

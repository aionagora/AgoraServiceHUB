namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.AuditLog;
using Microsoft.AspNetCore.Mvc;

using PaginatedAuditResultDto = AgoraHub360.ERP.Shared.DTOs.AuditLog.PaginatedResultDto<AgoraHub360.ERP.Shared.DTOs.AuditLog.AuditLogDto>;

public class AuditLogsControllerSecurityTests
{
    [Fact]
    public async Task AdminEmpresa_ConsultaLogsSoloDeTenantActivo()
    {
        var service = new FakeAuditLogService
        {
            LogsResult = new PaginatedAuditResultDto
            {
                Items = new List<AuditLogDto>
                {
                    new() { Id = 1, EmpresaId = 1, Entidad = "X", Accion = "U", EntidadId = "1", FechaHora = DateTime.UtcNow }
                }
            }
        };

        var sut = BuildController(service, new FakeCurrentUserService
        {
            PlatformRole = Roles.None,
            TenantRole = Roles.AdminEmpresa,
            TenantStatus = TenantStatus.Selected,
            TenantId = 1,
            EmpresaId = 1
        });

        var filter = new AuditLogFilterDto { EmpresaId = 999 };
        var result = await sut.GetLogs(filter);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PaginatedAuditResultDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.NotNull(service.LastFilter);
        Assert.Equal(1, service.LastFilter!.EmpresaId);
    }

    [Fact]
    public async Task AdminEmpresa_NoPuedePedirLogDeOtroTenant()
    {
        var service = new FakeAuditLogService
        {
            ByIdResult = new AuditLogDto
            {
                Id = 10,
                EmpresaId = 2,
                Entidad = "X",
                EntidadId = "2",
                Accion = "U",
                FechaHora = DateTime.UtcNow
            }
        };

        var sut = BuildController(service, new FakeCurrentUserService
        {
            PlatformRole = Roles.None,
            TenantRole = Roles.AdminEmpresa,
            TenantStatus = TenantStatus.Selected,
            TenantId = 1,
            EmpresaId = 1
        });

        var result = await sut.GetById(10);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task SecurityAuditor_PuedeLeerLogsGlobales()
    {
        var service = new FakeAuditLogService
        {
            LogsResult = new PaginatedAuditResultDto
            {
                Items = new List<AuditLogDto>
                {
                    new() { Id = 1, EmpresaId = 1, Entidad = "X", Accion = "U", EntidadId = "1", FechaHora = DateTime.UtcNow },
                    new() { Id = 2, EmpresaId = 2, Entidad = "Y", Accion = "C", EntidadId = "2", FechaHora = DateTime.UtcNow }
                }
            }
        };

        var sut = BuildController(service, new FakeCurrentUserService
        {
            PlatformRole = Roles.SecurityAuditor,
            TenantRole = Roles.NoAccess,
            TenantStatus = TenantStatus.NotSelected
        });

        var result = await sut.GetLogs(new AuditLogFilterDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PaginatedAuditResultDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data!.Items.Count);
        Assert.Null(service.LastFilter!.EmpresaId);
    }

    private static AuditLogsController BuildController(IAuditLogService service, ICurrentUserService currentUser)
        => new(service, currentUser);

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; }
        public int? UserIdInt { get; set; }
        public string? UserName { get; set; }
        public int? EmpresaId { get; set; }
        public int? TenantId { get; set; }
        public string PlatformRole { get; set; } = Roles.None;
        public string TenantRole { get; set; } = Roles.NoAccess;
        public string TenantStatus { get; set; } = AgoraHub360.ERP.Shared.Constants.TenantStatus.NotSelected;
        public bool IsInRole(string role) => false;
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public AuditLogFilterDto? LastFilter { get; private set; }
        public PaginatedAuditResultDto LogsResult { get; set; } = new();
        public AuditLogDto? ByIdResult { get; set; }
        public List<string> EntidadesResult { get; set; } = new();

        public Task<PaginatedAuditResultDto> GetLogsAsync(AuditLogFilterDto filter)
        {
            LastFilter = filter;
            return Task.FromResult(LogsResult);
        }

        public Task<AuditLogDto?> GetByIdAsync(long id) => Task.FromResult(ByIdResult);

        public Task<List<string>> GetEntidadesDistintasAsync(int? empresaId = null)
            => Task.FromResult(EntidadesResult);
    }
}

namespace AgoraHub360.ERP.Tests.Domain;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

public class AuditLogTests
{
    [Fact]
    public void AuditLog_DoesNotInheritAuditableEntity()
    {
        var log = new AuditLog();
        Assert.False(typeof(AuditableEntity).IsAssignableFrom(log.GetType()));
    }

    [Fact]
    public void AuditLog_DefaultValues()
    {
        var log = new AuditLog();

        Assert.Equal(0, log.Id);
        Assert.Equal(string.Empty, log.Entidad);
        Assert.Equal(string.Empty, log.EntidadId);
        Assert.Equal(string.Empty, log.Accion);
        Assert.Null(log.ValoresAnteriores);
        Assert.Null(log.ValoresNuevos);
        Assert.Null(log.CamposModificados);
        Assert.Null(log.EmpresaId);
        Assert.Null(log.Usuario);
    }

    [Fact]
    public void AuditLog_SetProperties()
    {
        var now = DateTime.UtcNow;
        var log = new AuditLog
        {
            Id = 42,
            Entidad = "Empresa",
            EntidadId = "1",
            Accion = "Insert",
            ValoresNuevos = "{\"Nombre\":\"Test\"}",
            EmpresaId = 1,
            Usuario = "admin",
            FechaHora = now
        };

        Assert.Equal(42, log.Id);
        Assert.Equal("Empresa", log.Entidad);
        Assert.Equal("1", log.EntidadId);
        Assert.Equal("Insert", log.Accion);
        Assert.Equal("{\"Nombre\":\"Test\"}", log.ValoresNuevos);
        Assert.Equal(1, log.EmpresaId);
        Assert.Equal("admin", log.Usuario);
        Assert.Equal(now, log.FechaHora);
    }

    [Fact]
    public void AuditLog_UpdateAction_HasBeforeAndAfterValues()
    {
        var log = new AuditLog
        {
            Accion = "Update",
            ValoresAnteriores = "{\"Nombre\":\"Antes\"}",
            ValoresNuevos = "{\"Nombre\":\"Despues\"}",
            CamposModificados = "Nombre"
        };

        Assert.Equal("Update", log.Accion);
        Assert.NotNull(log.ValoresAnteriores);
        Assert.NotNull(log.ValoresNuevos);
        Assert.Equal("Nombre", log.CamposModificados);
    }
}

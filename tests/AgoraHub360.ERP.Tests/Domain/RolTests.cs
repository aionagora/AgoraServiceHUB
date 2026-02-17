namespace AgoraHub360.ERP.Tests.Domain;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

public class RolTests
{
    [Fact]
    public void Rol_InheritsAuditableEntity()
    {
        var rol = new Rol();
        Assert.IsAssignableFrom<AuditableEntity>(rol);
    }

    [Fact]
    public void Rol_DefaultValues()
    {
        var rol = new Rol();
        Assert.True(rol.Activo);
        Assert.Equal(string.Empty, rol.Nombre);
        Assert.Null(rol.Descripcion);
    }

    [Fact]
    public void Rol_SetProperties()
    {
        var rol = new Rol
        {
            Id = 1,
            Nombre = "Admin",
            Descripcion = "Administrador total"
        };

        Assert.Equal(1, rol.Id);
        Assert.Equal("Admin", rol.Nombre);
        Assert.Equal("Administrador total", rol.Descripcion);
    }
}

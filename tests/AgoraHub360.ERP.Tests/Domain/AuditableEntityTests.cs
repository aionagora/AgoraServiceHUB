namespace AgoraHub360.ERP.Tests.Domain;

using AgoraHub360.ERP.Domain.Common;

public class AuditableEntityTests
{
    private class TestEntity : AuditableEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public void AuditableEntity_DefaultValues_ActivoIsTrue()
    {
        var entity = new TestEntity();
        Assert.True(entity.Activo);
    }

    [Fact]
    public void AuditableEntity_SetAuditFields_ValuesAreStored()
    {
        var now = DateTime.UtcNow;
        var entity = new TestEntity
        {
            FechaCreacion = now,
            CreadoPor = "admin",
            FechaModificacion = now,
            ModificadoPor = "admin"
        };

        Assert.Equal(now, entity.FechaCreacion);
        Assert.Equal("admin", entity.CreadoPor);
        Assert.Equal(now, entity.FechaModificacion);
        Assert.Equal("admin", entity.ModificadoPor);
    }
}

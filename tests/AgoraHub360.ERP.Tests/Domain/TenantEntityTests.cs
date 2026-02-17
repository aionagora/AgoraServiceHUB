namespace AgoraHub360.ERP.Tests.Domain;

using AgoraHub360.ERP.Domain.Common;

public class TenantEntityTests
{
    private class TestTenantEntity : TenantEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public void TenantEntity_InheritsAuditableEntity()
    {
        var entity = new TestTenantEntity();
        Assert.IsAssignableFrom<AuditableEntity>(entity);
    }

    [Fact]
    public void TenantEntity_EmpresaId_CanBeSet()
    {
        var entity = new TestTenantEntity { EmpresaId = 5 };
        Assert.Equal(5, entity.EmpresaId);
    }
}

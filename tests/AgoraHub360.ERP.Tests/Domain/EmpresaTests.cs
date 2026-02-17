namespace AgoraHub360.ERP.Tests.Domain;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

public class EmpresaTests
{
    [Fact]
    public void Empresa_InheritsAuditableEntity()
    {
        var empresa = new Empresa();
        Assert.IsAssignableFrom<AuditableEntity>(empresa);
    }

    [Fact]
    public void Empresa_IsNotTenantEntity()
    {
        // Empresa es la raiz del tenant, no hereda de TenantEntity
        var empresa = new Empresa();
        Assert.False(typeof(TenantEntity).IsAssignableFrom(empresa.GetType()));
    }

    [Fact]
    public void Empresa_DefaultValues_ActivoIsTrue()
    {
        var empresa = new Empresa();
        Assert.True(empresa.Activo);
        Assert.Equal(string.Empty, empresa.Nombre);
    }

    [Fact]
    public void Empresa_SetProperties_ValuesAreStored()
    {
        var empresa = new Empresa
        {
            Id = 1,
            Nombre = "Mi Empresa S.R.L.",
            NIT = "1234567890",
            Direccion = "Calle Principal 123",
            Telefono = "+591 77712345",
            Email = "info@miempresa.com",
            MonedaBaseId = "BOB"
        };

        Assert.Equal(1, empresa.Id);
        Assert.Equal("Mi Empresa S.R.L.", empresa.Nombre);
        Assert.Equal("1234567890", empresa.NIT);
        Assert.Equal("BOB", empresa.MonedaBaseId);
    }
}

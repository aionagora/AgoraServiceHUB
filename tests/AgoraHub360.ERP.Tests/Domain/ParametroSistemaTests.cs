namespace AgoraHub360.ERP.Tests.Domain;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

public class ParametroSistemaTests
{
    [Fact]
    public void ParametroSistema_InheritsTenantEntity()
    {
        var param = new ParametroSistema();
        Assert.IsAssignableFrom<TenantEntity>(param);
        Assert.IsAssignableFrom<AuditableEntity>(param);
    }

    [Fact]
    public void ParametroSistema_DefaultValues()
    {
        var param = new ParametroSistema();

        Assert.Equal(string.Empty, param.Clave);
        Assert.Equal(string.Empty, param.Valor);
        Assert.Null(param.Descripcion);
        Assert.Equal("General", param.Categoria);
        Assert.Equal("String", param.TipoDato);
        Assert.True(param.Activo);
        Assert.Equal(0, param.EmpresaId);
    }

    [Fact]
    public void ParametroSistema_SetProperties()
    {
        var param = new ParametroSistema
        {
            Id = 1,
            Clave = "MonedaBase",
            Valor = "BOB",
            Descripcion = "Moneda base de la empresa",
            Categoria = "General",
            TipoDato = "Select",
            EmpresaId = 5
        };

        Assert.Equal(1, param.Id);
        Assert.Equal("MonedaBase", param.Clave);
        Assert.Equal("BOB", param.Valor);
        Assert.Equal("Moneda base de la empresa", param.Descripcion);
        Assert.Equal(5, param.EmpresaId);
    }
}

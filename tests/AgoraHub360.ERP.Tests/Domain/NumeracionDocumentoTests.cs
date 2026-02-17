namespace AgoraHub360.ERP.Tests.Domain;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

public class NumeracionDocumentoTests
{
    [Fact]
    public void NumeracionDocumento_InheritsTenantEntity()
    {
        var num = new NumeracionDocumento();
        Assert.IsAssignableFrom<TenantEntity>(num);
        Assert.IsAssignableFrom<AuditableEntity>(num);
    }

    [Fact]
    public void NumeracionDocumento_DefaultValues()
    {
        var num = new NumeracionDocumento();

        Assert.Equal(string.Empty, num.TipoDocumento);
        Assert.Equal(string.Empty, num.Descripcion);
        Assert.Equal(string.Empty, num.Prefijo);
        Assert.Equal(1, num.SiguienteNumero);
        Assert.Equal(6, num.Digitos);
        Assert.True(num.Activo);
    }

    [Fact]
    public void GenerarSiguiente_FormatsCorrectly()
    {
        var num = new NumeracionDocumento
        {
            Prefijo = "OC-",
            SiguienteNumero = 1,
            Digitos = 6
        };

        var resultado = num.GenerarSiguiente();

        Assert.Equal("OC-000001", resultado);
        Assert.Equal(2, num.SiguienteNumero); // Avanza el contador
    }

    [Fact]
    public void GenerarSiguiente_MultipleCallsIncrement()
    {
        var num = new NumeracionDocumento
        {
            Prefijo = "FAC-",
            SiguienteNumero = 98,
            Digitos = 4
        };

        var r1 = num.GenerarSiguiente();
        var r2 = num.GenerarSiguiente();
        var r3 = num.GenerarSiguiente();

        Assert.Equal("FAC-0098", r1);
        Assert.Equal("FAC-0099", r2);
        Assert.Equal("FAC-0100", r3);
        Assert.Equal(101, num.SiguienteNumero);
    }

    [Fact]
    public void GenerarSiguiente_CustomDigits()
    {
        var num = new NumeracionDocumento
        {
            Prefijo = "NC-",
            SiguienteNumero = 42,
            Digitos = 8
        };

        Assert.Equal("NC-00000042", num.GenerarSiguiente());
    }
}

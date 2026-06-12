# 08 — Tests Unitarios (Tests)

> **Proyecto:** AgoraHub360.ERP.Tests  
> **Última actualización:** 2026-06-11  
> **Propósito:** Pruebas unitarias con xUnit, fakes manuales (sin Moq).

---

## 1. Estructura

```
tests/AgoraHub360.ERP.Tests/
├── AgoraHub360.ERP.Tests.csproj
├── GlobalUsings.cs
├── Application/
│   ├── FacturaVentaServiceTests.cs
│   └── (otros tests de servicios)
└── Infrastructure/
    └── CirrusFacturacionProviderTests.cs
```

---

## 2. Configuración del Proyecto

**Archivo:** `AgoraHub360.ERP.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.4.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/AgoraHub360.ERP.Application/AgoraHub360.ERP.Application.csproj" />
    <ProjectReference Include="../../src/AgoraHub360.ERP.Domain/AgoraHub360.ERP.Domain.csproj" />
    <ProjectReference Include="../../src/AgoraHub360.ERP.Infrastructure/AgoraHub360.ERP.Infrastructure.csproj" />
    <ProjectReference Include="../../src/AgoraHub360.ERP.Persistence/AgoraHub360.ERP.Persistence.csproj" />
    <ProjectReference Include="../../src/AgoraHub360.ERP.Shared/AgoraHub360.ERP.Shared.csproj" />
  </ItemGroup>
</Project>
```

**Dependencias:** xUnit 2.4.2, .NET Test SDK 17.9.0  
**Sin:** Moq, FluentAssertions, WebApplicationFactory

---

## 3. Tests por Módulo

### FacturaVentaServiceTests

**Archivo:** `Application/FacturaVentaServiceTests.cs`

| Test | Descripción |
|---|---|
| `GetAllAsync_PopulatesCollectionFields_WhenAssociatedCxcExists` | Verifica que al listar facturas, se llenen los campos de colección (SaldoPagado, etc.) cuando existen CxC asociadas |

### CirrusFacturacionProviderTests

**Archivo:** `Infrastructure/CirrusFacturacionProviderTests.cs`

| Test | Descripción |
|---|---|
| `CodigoProveedor_DeberiaSerCIRRUS` | Verifica que la propiedad `CodigoProveedor` retorne `"CIRRUS"` |
| `EmitirFacturaAsync_CuandoTokenFalla_DeberiaRetornarExitosoFalse` | Token endpoint retorna 401 |
| `EmitirFacturaAsync_CuandoRespuestaValidated_DeberiaRetornarExitosoTrue` | Cirrus responde con status VALIDATED |
| `EmitirFacturaAsync_CuandoRespuestaOffline_DeberiaRetornarExitosoTrueYEsOfflineTrue` | Cirrus responde OFFLINE |
| `EmitirFacturaAsync_CuandoRespuestaRejected_DeberiaRetornarExitosoFalse` | Cirrus responde REJECTED |
| `EmitirFacturaAsync_CuandoTimeout_DeberiaRetornarExitosoFalse` | Timeout simulado |

---

## 4. Patrón de Fakes

### FakeHttpMessageHandler

```csharp
private class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _handler(request);
    }
}
```

Se usa para simular respuestas HTTP del proveedor Cirrus sin llamar APIs reales.

### FakeHttpClientFactory

```csharp
private class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient;
    public HttpClient CreateClient(string name) => _httpClient;
}
```

### FakeCifradoService

```csharp
private class FakeCifradoService : ICifradoService
{
    public string Cifrar(string texto) => texto;
    public string Descifrar(string textoCifrado)
    {
        if (textoCifrado.StartsWith("v1:"))
            return textoCifrado[3..];
        return textoCifrado;
    }
}
```

### FakeRepo<T>

```csharp
// Repositorio fake genérico con lista en memoria
private class FakeRepo<T> : IRepository<T> where T : class
{
    private readonly List<T> _data = new();
    // Implementa todos los métodos de IRepository<T>
}
```

### FakeCxcService

```csharp
// Implementa ICuentasPorCobrarService con datos dummy
private class FakeCxcService : ICuentasPorCobrarService
{
    public Task<Result<PaginatedResultDto<CuentaPorCobrarResumenDto>>> GetAllAsync(...) => ...;
    public Task<Result<decimal>> GetSaldoTotalPendienteAsync(...) => Result.Success(0m);
    // ... otros métodos
}
```

---

## 5. Estadísticas

| Métrica | Valor |
|---|---|
| Test framework | xUnit 2.4.2 |
| Total tests | **125** |
| Tests passed | 125 |
| Tests failed | 0 |
| Tests skipped | 0 |
| Tiempo de ejecución | ~3.6s |
| Clases de test | ~4 (parcialmente exploradas) |
| Fakes manuales | FakeRepo, FakeHttpMessageHandler, FakeHttpClientFactory, FakeCifradoService |

---

## 6. Cobertura por Módulo

| Módulo | Tests |
|---|---|
| Application — FacturaVentaService | 1 test (colecciones CxC) |
| Infrastructure — CirrusFacturacionProvider | 6 tests (token, validado, offline, rechazado, timeout, código) |
| Otros módulos | No se detectaron tests adicionales en la exploración |

---

## 7. Reglas

| Regla | Aplicación |
|---|---|
| No Moq | ✅ Todos los fakes son manuales |
| No FluentAssertions | ✅ Solo xUnit `Assert` |
| No WebApplicationFactory | ✅ Sin tests de integración HTTP |
| No llamar APIs reales | ✅ Fakes HTTP en tests de provider |
| No exponer secretos | ✅ Fakes usan datos dummy |
| Tests independientes | ✅ Cada test usa su propio state |
| `[Fact]` (no `[Theory]`) | ✅ Todos los tests son `[Fact]` |

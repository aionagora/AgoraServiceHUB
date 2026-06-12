# 07 — Capa de Infraestructura (Infrastructure)

> **Proyecto:** AgoraHub360.ERP.Infrastructure  
> **Última actualización:** 2026-06-11  
> **Propósito:** Servicios externos, integraciones, cifrado, proveedores de facturación electrónica.

---

## 1. Estructura

```
src/AgoraHub360.ERP.Infrastructure/
├── Services/
│   ├── AesCifradoService.cs           ← Cifrado AES-256-CBC
│   ├── ExportService.cs               ← Exportación a Excel/PDF
│   ├── AsientoExportService.cs        ← Exportación de asientos contables
│   └── AsientoImportService.cs        ← Importación de asientos
├── FacturacionElectronica/
│   └── CirrusFacturacionProvider.cs   ← Provider FE para Cirrus
├── DependencyInjection.cs
└── (otros servicios externos futuros)
```

---

## 2. AesCifradoService

**Archivo:** `Services/AesCifradoService.cs`
**Implementa:** `ICifradoService`

```csharp
public class AesCifradoService : ICifradoService
```

### Especificaciones

| Aspecto | Detalle |
|---|---|
| Algoritmo | AES-256-CBC |
| Tamaño de clave | 32 bytes (256 bits) |
| IV | 16 bytes aleatorios por cifrado |
| Formato de salida | `v1:{Base64(IV + Ciphertext)}` |
| Padding | PKCS7 |
| Configuración | `Encryption:Key` (Base64) desde `appsettings` o User Secrets |

### Métodos

```csharp
string Cifrar(string texto);
string Descifrar(string textoCifrado);
```

### Reglas de Seguridad

- La clave `Encryption:Key` **nunca** está hardcodeada en el repositorio.
- No se loguean secretos ni texto plano ni cifrado.
- El prefijo `v1:` permite versionar el formato de cifrado en el futuro.
- Errores de descifrado lanzan `InvalidOperationException` con mensaje genérico.

### Dependencias

```csharp
public AesCifradoService(IConfiguration configuration, ILogger<AesCifradoService> logger)
```

---

## 3. CirrusFacturacionProvider

**Archivo:** `FacturacionElectronica/CirrusFacturacionProvider.cs`
**Implementa:** `IFacturacionElectronicaProvider`

```csharp
public class CirrusFacturacionProvider : IFacturacionElectronicaProvider
{
    public string CodigoProveedor => "CIRRUS";
}
```

### Dependencias

```csharp
public CirrusFacturacionProvider(
    IHttpClientFactory httpClientFactory,
    ICifradoService cifradoService,
    ILogger<CirrusFacturacionProvider> logger)
```

### Funcionalidades

| Método | Endpoint Cirrus |
|---|---|
| `EmitirFacturaAsync` | `POST {ApiBillingUrl}/bill/sales` |
| `AnularFacturaAsync` | `GET {ApiManagementUrl}/api/v1/bill/void?cuf=...&motivo=...` |
| `VerificarEstadoAsync` | `GET {ApiManagementUrl}/api/v1/bill/status?cuf=...` |
| `ObtenerCufdAsync` | `GET {ApiManagementUrl}/api/v1/bill/cufd` |

### OAuth2 client_credentials

1. Descifra `ClientSecretEncrypted` usando `ICifradoService`
2. POST a `config.TokenUrl` con `grant_type=client_credentials`
3. Obtiene `access_token`, `expires_in`
4. Cachea token en `ConcurrentDictionary<int, (string, DateTime)>` con margen de 5 min
5. Usa `SemaphoreSlim` para evitar renovaciones simultáneas

### Headers de Emisión

```
Authorization: Bearer {access_token}
Authorization-Pos-Token: {pos_token_descifrado}
```

### Mapeo de Estados Cirrus → SIAT

| Cirrus | Resultado | Estado SIAT |
|---|---|---|
| `VALIDATED` | Exitoso | `Validada` |
| `OFFLINE` | Exitoso (EsOffline=true) | `Pendiente` |
| `REJECTED` | Error de negocio | `Rechazada` |
| `ERROR` / Timeout | Error técnico | `NoEnviada` |

### Modelos Internos (privados)

| Modelo | Propósito |
|---|---|
| `CirrusTokenResponse` | Respuesta OAuth2 |
| `CirrusBillRequest` | Request de emisión (snake_case) |
| `CirrusItemRequest` | Item de línea para emisión |
| `CirrusApiResponse<T>` | Envoltura de respuesta Cirrus |
| `CirrusBillResponse` | Respuesta de factura emitida |
| `CirrusVoidResponse` | Respuesta de anulación |
| `CirrusStatusResponse` | Respuesta de consulta de estado |
| `CirrusCufdResponse` | Respuesta de CUFD |

### JSON Serialization

```csharp
private static readonly JsonSerializerOptions _jsonOptions = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

---

## 4. IHttpClientFactory

```csharp
services.AddHttpClient("CirrusFacturacion");
```

Se usa `IHttpClientFactory` (no `new HttpClient()`).  
Timeout configurable por empresa desde `ConfiguracionFacturacionElectronica.TimeoutSegundos` (mínimo 5, máximo 60).

---

## 5. Logging Seguro

| Elemento | ¿Se loguea? |
|---|---|
| `BillUuid` | ✅ Sí |
| `EmpresaId` | ✅ Sí |
| `ConfiguracionId` | ✅ Sí |
| `CodigoProveedor` | ✅ Sí |
| Tiempo de respuesta | ✅ Sí |
| `access_token` | ❌ No |
| `ClientSecret` / `ClientSecretEncrypted` | ❌ No |
| `PosToken` / `PosTokenEncrypted` | ❌ No |
| Request body completo | ❌ No |
| Response body completo | ❌ No |
| CUF completo | ❌ No (solo últimos 6 caracteres en errores) |

---

## 6. ExportService

**Archivo:** `Services/ExportService.cs`
**Implementa:** `IExportService`

Provee exportación a Excel y PDF de listados del sistema.

---

## 7. DependencyInjection

```csharp
services.AddScoped<IExportService, ExportService>();
services.AddScoped<IAsientoExportService, AsientoExportService>();
services.AddScoped<IAsientoImportService, AsientoImportService>();

// Cifrado de secretos
services.AddScoped<ICifradoService, AesCifradoService>();

// Provider FE
services.AddHttpClient("CirrusFacturacion");
services.AddScoped<IFacturacionElectronicaProvider, CirrusFacturacionProvider>();
```

---

## 8. Reglas de Seguridad de Secretos

| Regla | Aplicación |
|---|---|
| Secretos cifrados en BD | ✅ `ClientSecretEncrypted` y `PosTokenEncrypted` con formato `v1:...` |
| Clave de cifrado externa | ✅ `Encryption:Key` desde configuración (no hardcodeada) |
| Sin secretos en logs | ✅ Verificado en CirrusFacturacionProvider |
| Sin secretos en API responses | ✅ `ConfiguracionFEDto` no expone valores |
| Sin secretos en UI | ✅ Formularios muestran input type="password" vacío |
| Descifrado solo para operación | ✅ Solo en `ObtenerTokenAsync` y `DescifrarPosToken` |

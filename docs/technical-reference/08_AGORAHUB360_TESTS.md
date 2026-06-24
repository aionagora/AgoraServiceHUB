# AGORAHUB360 ERP — PRUEBAS

> **Proyecto:** `tests/AgoraHub360.ERP.Tests/`
> **Propósito:** Pruebas unitarias de servicios de aplicación y dominio, más pruebas de seguridad de controllers.

---

## 1. Estructura

```
AgoraHub360.ERP.Tests/
├── Application/        ← Pruebas de servicios de aplicación y seguridad de controllers
│   ├── AuditLogDtoTests.cs
│   ├── AuditLogsControllerSecurityTests.cs
│   ├── AuthControllerSecurityTests.cs
│   ├── BranchAccessHandlerTests.cs
│   ├── ClienteCreditoConfiguracionServiceTests.cs
│   ├── CuentasPorCobrarServiceTests.cs
│   ├── EmpresaDemoServiceTests.cs
│   ├── EmpresaServiceTests.cs
│   ├── EmpresasControllerSecurityTests.cs
│   ├── FacturaVentaServiceTests.cs
│   ├── NumeracionDocumentoServiceTests.cs
│   ├── NumeracionesControllerSecurityTests.cs
│   ├── ParametroSistemaServiceTests.cs
│   ├── ParametrosControllerSecurityTests.cs
│   ├── ResultTests.cs
│   ├── RolesControllerSecurityTests.cs
│   ├── RolServiceTests.cs
│   ├── SeguridadDinamicaControllerSecurityTests.cs
│   ├── SecurityPolicyNamesTests.cs
│   ├── SucursalesControllerSecurityTests.cs
│   ├── TenantMembershipHandlerTests.cs
│   ├── UsuariosControllerSecurityTests.cs
│   └── VentaServiceTests.cs
├── Domain/              ← Pruebas de entidades de dominio
│   ├── AuditableEntityTests.cs
│   ├── AuditLogTests.cs
│   ├── EmpresaTests.cs
│   ├── NumeracionDocumentoTests.cs
│   ├── ParametroSistemaTests.cs
│   ├── RolTests.cs
│   └── TenantEntityTests.cs
├── GlobalUsings.cs
└── AgoraHub360.ERP.Tests.csproj
```

---

## 2. Matriz de Cobertura

| Test Class | Módulo | Tipo | Cobertura |
|---|---|---|---|
| `AuditableEntityTests` | Domain/Core | Unitario | Propiedades de auditoría |
| `AuditLogTests` | Domain/Core | Unitario | Entidad AuditLog |
| `EmpresaTests` | Domain/Core | Unitario | Entidad Empresa, configuración de industria |
| `NumeracionDocumentoTests` | Domain/Core | Unitario | Secuencias numéricas |
| `ParametroSistemaTests` | Domain/Core | Unitario | Parámetros del sistema |
| `RolTests` | Domain/Core | Unitario | Roles y permisos |
| `TenantEntityTests` | Domain/Core | Unitario | Herencia TenantEntity, EmpresaId |
| `ResultTests` | Application/Common | Unitario | Result<T>, Success/Failure |
| `AuditLogDtoTests` | Application/AuditLog | Unitario | Mapeo DTOs |
| `ClienteCreditoConfiguracionServiceTests` | Application/CxC | Unitario | Configuración de crédito |
| `CuentasPorCobrarServiceTests` | Application/CxC | Unitario | Generación, pagos, antigüedad |
| `EmpresaDemoServiceTests` | Application/Empresa | Unitario | Creación de empresa demo |
| `EmpresaServiceTests` | Application/Empresa | Unitario | CRUD empresas |
| `FacturaVentaServiceTests` | Application/Ventas | Unitario | Generación y anulación de facturas |
| `NumeracionDocumentoServiceTests` | Application/Core | Unitario | Secuencias de numeración |
| `ParametroSistemaServiceTests` | Application/Core | Unitario | CRUD parámetros |
| `RolServiceTests` | Application/Core | Unitario | CRUD roles |
| `VentaServiceTests` | Application/Ventas | Unitario | CRUD ventas, confirmación, pagos |
| `AuthControllerSecurityTests` | API/Security | Seguridad | Acceso anónimo vs autenticado |
| `AuditLogsControllerSecurityTests` | API/Security | Seguridad | Políticas de acceso |
| `EmpresasControllerSecurityTests` | API/Security | Seguridad | Roles y permisos |
| `NumeracionesControllerSecurityTests` | API/Security | Seguridad | Políticas de acceso |
| `ParametrosControllerSecurityTests` | API/Security | Seguridad | Políticas de acceso |
| `RolesControllerSecurityTests` | API/Security | Seguridad | Políticas de acceso |
| `SeguridadDinamicaControllerSecurityTests` | API/Security | Seguridad | Políticas de acceso |
| `SucursalesControllerSecurityTests` | API/Security | Seguridad | Políticas de acceso |
| `UsuariosControllerSecurityTests` | API/Security | Seguridad | Políticas de acceso |
| `BranchAccessHandlerTests` | API/Authorization | Unitario | Acceso por sucursal |
| `TenantMembershipHandlerTests` | API/Authorization | Unitario | Membresía tenant |
| `SecurityPolicyNamesTests` | API/Security | Unitario | Validación de nombres de políticas |

---

## 3. Seguridad Testeada

### Controllers con tests de seguridad

| Controller | Políticas validadas |
|---|---|
| `AuthController` | Endpoints anónimos vs autenticados |
| `EmpresasController` | PlatformSuperAdmin, TenantAdmin |
| `UsuariosController` | Roles y permisos de usuario |
| `RolesController` | Roles y permisos |
| `SucursalesController` | Acceso por empresa |
| `NumeracionesController` | Configuración de numeración |
| `ParametrosController` | Parámetros del sistema |
| `AuditLogsController` | Acceso a logs |
| `SeguridadDinamicaController` | Seguridad dinámica |

### Handlers de autorización testeados

| Handler | Prueba |
|---|---|
| `TenantMembershipHandler` | Verifica que el usuario pertenezca al tenant |
| `BranchAccessHandler` | Verifica acceso por sucursal |

---

## 4. Tenant Tests

| Test | Descripción |
|---|---|
| `TenantEntityTests` | Verifica que `TenantEntity` herede correctamente `EmpresaId` |
| `TenantMembershipHandlerTests` | Verifica que el handler autorice/rechace según membresía |
| Múltiples security tests | Verifican que endpoints requieran `EmpresaId` claim |

---

## 5. Hallazgos

| Hallazgo | Impacto | Recomendación |
|---|---|---|
| Cobertura limitada a servicios críticos | Medio | Extender tests a servicios de contabilidad, compras e inventario |
| Sin tests de integración con BD | Alto | Implementar tests de integración con base de datos en memoria |
| Sin tests de UI (Blazor) | Medio | Considerar bUnit para componentes Blazor |
| Handlers de autorización cubiertos | Bueno | Mantener cobertura actual |
| Tests de seguridad cubren 9 controllers | Bueno | Extender a los ~45 controllers restantes |

---

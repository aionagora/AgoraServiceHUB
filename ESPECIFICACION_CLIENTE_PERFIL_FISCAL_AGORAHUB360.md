# Especificación Funcional y Técnica
# Módulo: Perfil Fiscal de Cliente
## AgoraHUB360 ERP

---

## 1. Objetivo

Diseñar e implementar un modelo formal para administrar múltiples perfiles fiscales por cliente dentro de AgoraHUB360 ERP.

Este módulo permitirá separar claramente:

- Cliente comercial
- Cliente operativo
- Cliente sucursal
- Perfil fiscal / datos de facturación
- Snapshot fiscal de venta
- Snapshot legal de factura

El objetivo principal es preparar el sistema para facturación interna, facturación electrónica futura, consumo por API fiscal, SIAT/SIN, cuentas por cobrar, reportes tributarios, ventas B2B, ventas a terceros y clientes con varias razones sociales o NIT.

---

## 2. Problema Actual

Actualmente el sistema usa principalmente datos del cliente para facturar:

- NIT
- Razón Social
- Email
- Teléfono

Sin embargo, en la operación real, no siempre se factura al mismo nombre del cliente comercial.

Ejemplo:

```text
Cliente comercial: Clínica Los Andes

Perfiles fiscales posibles:
1. Clínica Los Andes SRL — NIT 123456
2. Fundación Los Andes — NIT 998877
3. Juan Pérez — CI 554433
4. Aseguradora Salud Total — NIT 778899
```

Esto implica que el cliente comercial puede ser uno, pero el destinatario fiscal puede ser otro.

---

## 3. Decisión Arquitectónica

Se define una nueva entidad maestra:

```text
ClientePerfilFiscal
```

Esta entidad representa los datos fiscales reutilizables asociados a un cliente.

No se debe reemplazar `Cliente`.

No se debe eliminar `VentaFacturacionDatos`.

No se debe eliminar `FacturaVenta`.

Flujo correcto:

```text
Cliente
   ↓
ClientePerfilFiscal
   ↓
VentaFacturacionDatos
   ↓
FacturaVenta
   ↓
API Facturación / SIAT futuro
```

---

## 4. Diferencia Conceptual entre Entidades

### Cliente

Representa al cliente comercial.

Responsable de relación comercial, contacto principal, clasificación, cuenta comercial, relación con sucursales, ventas y CRM.

### ClienteSucursal

Representa una ubicación, punto operativo o sucursal del cliente.

Responsable de dirección, contacto, logística, entrega, atención comercial y recepción de productos o servicios.

No debe ser la fuente principal de identidad fiscal.

### ClientePerfilFiscal

Representa una identidad fiscal o perfil de facturación reutilizable.

Responsable de NIT / CI / documento fiscal, razón social, complemento, correo fiscal, tipo de documento, configuración futura para API fiscal y dato predeterminado de facturación.

### VentaFacturacionDatos

Representa el snapshot fiscal usado en una venta.

Responsable de congelar los datos fiscales al momento de la venta.

### FacturaVenta

Representa el snapshot fiscal/legal definitivo.

Responsable de alimentar impresión, PDF futuro, API fiscal, SIAT futuro, auditoría y reportes tributarios.

---

## 5. Nombre Técnico Recomendado

Entidad recomendada:

```text
ClientePerfilFiscal
```

Alternativas aceptables:

```text
ClienteDatoFacturacion
ClientePerfilFacturacion
ClienteIdentidadFiscal
```

Recomendación final:

```text
ClientePerfilFiscal
```

Motivo:

- Es más amplio que “dato”.
- Permite crecer hacia SIAT/API.
- Representa una identidad fiscal completa.
- Permite múltiples perfiles por cliente.

---

## 6. Campos Propuestos

### ClientePerfilFiscal

| Campo | Tipo | Requerido | Descripción |
|---|---:|---:|---|
| Id | long | Sí | Identificador |
| EmpresaId | long | Sí | Tenant desde JWT |
| ClienteId | long | Sí | Cliente comercial asociado |
| ClienteSucursalId | long? | No | Sucursal relacionada opcional |
| Alias | string | Sí | Nombre corto para combo |
| TipoDocumentoIdentidad | string | Sí | NIT, CI, Pasaporte, Otro |
| NumeroDocumento | string | Sí | NIT/CI/documento |
| Complemento | string? | No | Complemento CI |
| RazonSocial | string | Sí | Nombre/Razón Social fiscal |
| TipoPersona | string? | No | Natural/Juridica |
| TipoPerfilFiscal | string? | No | Principal, Sucursal, Aseguradora, Gobierno, Particular, Otro |
| EmailFactura | string? | No | Correo para factura electrónica |
| TelefonoFactura | string? | No | Teléfono fiscal |
| RequiereEmail | bool | No | Exige email para facturación electrónica futura |
| EsPredeterminado | bool | Sí | Perfil fiscal principal |
| ValidadoFacturacion | bool | Sí | Validado por sistema externo o API fiscal |
| CodigoClienteApi | string? | No | Código para API de facturación |
| CodigoExternoFacturacion | string? | No | Código externo fiscal |
| Observaciones | string? | No | Nota interna |
| Activo | bool | Sí | Soft delete / vigencia |

---

## 7. Reglas Funcionales

### Regla 1 — Un cliente puede tener varios perfiles fiscales

Un cliente puede tener N perfiles fiscales activos.

### Regla 2 — Un perfil fiscal predeterminado

Cada cliente puede tener un perfil fiscal predeterminado.

Regla inicial:

```text
Solo uno activo como EsPredeterminado = true
```

Cuando se marca uno como predeterminado, los demás se desmarcan.

### Regla 3 — Primer perfil automático

Si se crea el primer perfil fiscal de un cliente:

```text
EsPredeterminado = true
```

automáticamente.

### Regla 4 — No aceptar EmpresaId desde frontend

`EmpresaId` se obtiene exclusivamente desde:

```text
ICurrentUserService.EmpresaId
```

Nunca desde DTO ni frontend.

### Regla 5 — Validar tenant

Todo perfil fiscal debe validar:

```text
Cliente.EmpresaId == EmpresaId actual
ClienteSucursal.EmpresaId == EmpresaId actual
ClienteSucursal.ClienteId == ClienteId
```

### Regla 6 — Snapshot en venta

Al seleccionar un perfil fiscal en una venta, se copian sus datos hacia `VentaFacturacionDatos`.

La venta no debe depender dinámicamente del perfil fiscal.

### Regla 7 — Snapshot en factura

Al generar factura, `FacturaVenta` debe copiar los datos fiscales desde `VentaFacturacionDatos`.

### Regla 8 — Fallback controlado

Orden de resolución fiscal recomendado:

```text
1. Request explícito de facturación
2. VentaFacturacionDatos
3. ClientePerfilFiscal seleccionado
4. ClientePerfilFiscal predeterminado
5. Cliente
```

---

## 8. Flujo Operativo en Ventas

1. Usuario selecciona cliente comercial.
2. Sistema carga perfiles fiscales activos del cliente.
3. Sistema selecciona automáticamente el perfil predeterminado.
4. Usuario puede usar el predeterminado, elegir otro perfil, registrar perfil fiscal rápido o ingresar dato ocasional.
5. El formulario copia datos al bloque de facturación.
6. Al guardar venta, se guarda snapshot en `VentaFacturacionDatos`.
7. Al generar factura, se copia snapshot hacia `FacturaVenta`.

---

## 9. Flujo de Registro Rápido

Desde la venta, debe existir botón:

```text
Nuevo perfil fiscal
```

Campos mínimos:

- Alias
- TipoDocumentoIdentidad
- NumeroDocumento
- Complemento
- RazonSocial
- EmailFactura
- TelefonoFactura
- EsPredeterminado

Al guardar:

- crea el perfil fiscal,
- recarga lista,
- selecciona automáticamente el nuevo perfil,
- copia datos al bloque de facturación.

---

## 10. Impacto en VentaFacturacionDatos

Agregar campo opcional:

```text
ClientePerfilFiscalId
```

Campos snapshot actuales deben mantenerse:

- TipoDocumentoIdentidad
- NitFactura
- Complemento
- RazonSocialFactura
- EmailFactura
- TelefonoFactura

Motivo:

```text
ClientePerfilFiscalId = referencia
Snapshot = dato legal usado en esa venta
```

---

## 11. Impacto en FacturaVenta

Agregar campo opcional:

```text
ClientePerfilFiscalId
```

Mantener snapshot:

- NitFactura
- Complemento
- RazonSocialFactura
- EmailFactura
- TelefonoFactura

La API fiscal futura debe consumir desde `FacturaVenta`.

---

## 12. API Propuesta

```text
GET    /api/v1/clientes/{clienteId}/perfiles-fiscales
GET    /api/v1/clientes/perfiles-fiscales/{id}
POST   /api/v1/clientes/{clienteId}/perfiles-fiscales
PUT    /api/v1/clientes/perfiles-fiscales/{id}
DELETE /api/v1/clientes/perfiles-fiscales/{id}
POST   /api/v1/clientes/perfiles-fiscales/{id}/predeterminado
```

---

## 13. DTOs Propuestos

### ClientePerfilFiscalDto

- Id
- ClienteId
- ClienteSucursalId
- Alias
- TipoDocumentoIdentidad
- NumeroDocumento
- Complemento
- RazonSocial
- TipoPersona
- TipoPerfilFiscal
- EmailFactura
- TelefonoFactura
- RequiereEmail
- EsPredeterminado
- ValidadoFacturacion
- CodigoClienteApi
- CodigoExternoFacturacion
- Observaciones
- Activo

### CrearClientePerfilFiscalRequestDto

No debe incluir EmpresaId.

- ClienteSucursalId
- Alias
- TipoDocumentoIdentidad
- NumeroDocumento
- Complemento
- RazonSocial
- TipoPersona
- TipoPerfilFiscal
- EmailFactura
- TelefonoFactura
- RequiereEmail
- EsPredeterminado
- CodigoClienteApi
- CodigoExternoFacturacion
- Observaciones

### ActualizarClientePerfilFiscalRequestDto

Campos similares al request de creación.

### EstablecerClientePerfilFiscalPredeterminadoRequestDto

Puede ser vacío o contener:

- ConfirmarCambio

---

## 14. Validaciones

### Creación

Validar:

- Cliente existe y pertenece a empresa activa.
- ClienteSucursal, si viene, pertenece al cliente y empresa activa.
- TipoDocumentoIdentidad requerido.
- NumeroDocumento requerido.
- RazonSocial requerida.
- Alias requerido.
- No duplicar mismo cliente + documento + razón social activos.
- Si es primer perfil, marcar predeterminado.

### Actualización

Validar:

- Perfil existe.
- Perfil pertenece a empresa activa.
- Cliente no se cambia directamente.
- Si cambia sucursal, validar pertenencia.
- Mantener unicidad funcional.

### Eliminación

Debe ser soft delete:

```text
Activo = false
```

No eliminar físico.

No permitir eliminar el único perfil predeterminado activo sin reasignar otro.

### Predeterminado

Al marcar un perfil como predeterminado:

```text
UPDATE otros perfiles activos del cliente
SET EsPredeterminado = false
```

Luego:

```text
perfil.EsPredeterminado = true
```

---

## 15. Multiempresa

Todas las consultas deben estar filtradas por EmpresaId actual.

Reglas:

- No aceptar EmpresaId en DTO.
- No devolver perfiles fiscales de otra empresa.
- No permitir usar ClienteId de otra empresa.
- No permitir usar ClienteSucursalId de otra empresa.
- No permitir usar ClientePerfilFiscalId de otra empresa.

---

## 16. Base de Datos

### Tabla propuesta

```text
mdm.ClientePerfilesFiscales
```

### Índices

```text
IX_ClientePerfilFiscal_Empresa_Cliente
EmpresaId, ClienteId

IX_ClientePerfilFiscal_Empresa_Documento
EmpresaId, NumeroDocumento

IX_ClientePerfilFiscal_Empresa_Cliente_Documento
EmpresaId, ClienteId, NumeroDocumento

IX_ClientePerfilFiscal_Empresa_Cliente_EsPredeterminado
EmpresaId, ClienteId, EsPredeterminado
```

### Índice filtrado futuro recomendado

```sql
CREATE UNIQUE INDEX UX_ClientePerfilFiscal_Predeterminado
ON mdm.ClientePerfilesFiscales(EmpresaId, ClienteId)
WHERE EsPredeterminado = 1 AND Activo = 1;
```

No implementarlo en primera fase si se busca menor riesgo.

---

## 17. Impacto en Frontend

### Módulo Cliente

Agregar sección:

```text
Perfiles fiscales
```

Dentro del detalle/formulario de cliente.

Debe permitir listar, crear, editar, desactivar y marcar predeterminado.

### Formulario Venta

En bloque facturación:

- mostrar selector “Facturar a”.
- cargar perfiles fiscales del cliente.
- seleccionar predeterminado automáticamente.
- permitir registro rápido.
- permitir edición manual para esa venta.
- guardar snapshot.

---

## 18. Impacto en Facturación Interna

`FacturaVentaService` debe resolver datos fiscales así:

```text
Request explícito
VentaFacturacionDatos
ClientePerfilFiscal
Cliente
```

Pero la factura generada siempre debe quedar congelada.

---

## 19. Impacto en API Fiscal Futura

La API fiscal debe consumir desde:

```text
FacturaVenta
```

No desde:

```text
Cliente
ClientePerfilFiscal
Venta
```

Motivo:

`FacturaVenta` es el documento legal final.

---

## 20. Fases de Implementación

### Fase 1 — Domain + Persistence

- Crear `ClientePerfilFiscal`.
- Crear configuración EF.
- Crear migración.
- Agregar DbSet.

### Fase 2 — DTOs + Service + Controller

- Crear DTOs.
- Crear `IClientePerfilFiscalService`.
- Crear `ClientePerfilFiscalService`.
- Crear controller.
- Registrar DI.

### Fase 3 — Integración Ventas

- Agregar `ClientePerfilFiscalId` a `VentaFacturacionDatos`.
- Agregar DTO.
- Actualizar `VentaService`.

### Fase 4 — Integración FacturaVenta

- Agregar `ClientePerfilFiscalId` a `FacturaVenta`.
- Actualizar `FacturaVentaService`.

### Fase 5 — Frontend Ventas

- Crear servicio HTTP.
- Integrar selector “Facturar a”.
- Registro rápido.

### Fase 6 — Frontend MDM Cliente

- Sección de perfiles fiscales en cliente.
- CRUD completo.

---

## 21. Criterios de Aceptación

### Backend

- Un cliente puede tener varios perfiles fiscales.
- Solo uno puede quedar como predeterminado por cliente.
- No se permite cruce multiempresa.
- Venta puede guardar snapshot desde perfil fiscal.
- Factura puede guardar snapshot fiscal definitivo.

### Frontend

- Al seleccionar cliente, carga perfiles fiscales.
- Selecciona predeterminado automáticamente.
- Permite elegir otro.
- Permite crear uno rápido.
- Permite usar dato ocasional.
- Genera factura con datos correctos.

---

## 22. Decisiones de No Alcance Inicial

No implementar todavía:

- SIAT real.
- validación NIT en línea.
- PDF fiscal.
- índice filtrado complejo si no es necesario.
- sincronización con API externa.
- bloqueo tributario avanzado.
- historial de cambios fiscales.

---

## 23. Conclusión

`ClientePerfilFiscal` es una pieza clave para que AgoraHUB360 ERP pueda manejar facturación real en escenarios empresariales.

Permite separar correctamente:

```text
Cliente comercial
Cliente operativo
Perfil fiscal
Venta
Factura
API fiscal
```

Este diseño evita refactorizaciones futuras y deja el sistema preparado para SIAT, facturación electrónica, cuentas por cobrar, auditoría y reportes tributarios.

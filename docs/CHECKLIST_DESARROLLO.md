# AgoraHub360 ERP — Checklist de Desarrollo

> **Fecha:** 11 de mayo 2026  
> **Leyenda:** ✅ Completo · 🔄 En progreso · 🔲 Pendiente · ⬛ Bloqueado/Depende de otro

---

## FASE 1 — Base del sistema (v1.0.0) ✅ COMPLETADA

### 1.1 Core — Infraestructura base

- [x] Solución Clean Architecture (Domain / Application / Persistence / API / Web)
- [x] DbContext multi-tenant con Global Query Filter por `EmpresaId`
- [x] `AuditableEntity` + `TenantEntity` como clases base
- [x] `AuditableEntityInterceptor` (auto-rellena fechas y usuario)
- [x] `EntityVersioningInterceptor` (snapshots JSON en `EntityVersions`)
- [x] Soft delete global (`Activo = false`)
- [x] `Result<T>` pattern en servicios
- [x] `ApiResponse<T>` uniforme en todos los endpoints
- [x] Repository pattern genérico + Unit of Work
- [x] JWT Bearer Authentication (HS256, 8h expiración)
- [x] `StubAuthHandler` para desarrollo sin validar firma
- [x] `GlobalExceptionMiddleware` — respuesta JSON uniforme en errores
- [x] `TenantRequiredMiddleware` — valida `EmpresaId` en claims
- [x] API versionada (URL segment `/api/v1/` + Header)
- [x] CORS configurado para Blazor WASM
- [x] Swagger/OpenAPI (solo en Development)
- [x] Health check endpoint (`/health`)
- [x] `PeriodoContableNotificadorService` (background service)

### 1.2 Core — Módulo usuarios y empresas

- [x] CRUD Empresas (API + Web)
- [x] CRUD Sucursales de empresa (API + Web) — *PR #53–55*
- [x] CRUD Usuarios (API + Web)
- [x] CRUD Roles (API + Web)
- [x] Asignación usuario → empresa → rol
- [x] Acceso por sucursal (`UsuarioSucursalAcceso`)
- [x] Parámetros de sistema clave-valor (API + Web)
- [x] Numeración automática de documentos (API + Web)
- [x] Consulta de auditoría paginada (API + Web)
- [x] Monedas (seed inicial BOB/USD/EUR)

### 1.3 MDM — Master Data Management

- [x] Catálogos globales (API + Web)
- [x] Productos globales con wizard 6 pestañas (API + Web)
- [x] Activación de producto por empresa (`CompanyProduct`)
- [x] Features activables por empresa (LOT, SERIAL, FEFO, HAZMAT, QC, BOM…)
- [x] Variantes de producto con ejes de atributo
- [x] Atributos dinámicos EAV (7 tipos de dato + Select con opciones)
- [x] Códigos de producto (SKU, EAN, proveedor, cliente, canal…)
- [x] UdM alternativas con factores de conversión
- [x] Categorías jerárquicas N niveles
- [x] Marcas y Fabricantes globales (API + Web)
- [x] Clientes con datos fiscales (API + Web)
- [x] Sucursales de cliente (API + Web) — *PR #54–55*
- [x] Proveedores con datos fiscales (API + Web)
- [x] Almacenes (API + Web)
- [x] Listas de precios con ítems y vigencia (API + Web)
- [x] Industrias y reglas por industria (API, sin UI)

### 1.4 INV — Inventario

- [x] Movimientos de inventario: Entrada, Salida, Ajuste, Transferencia
- [x] Stock por producto-almacén con Costo Promedio Ponderado (WAC)
- [x] Kardex con trazabilidad completa
- [x] Integración automática: recepción de compra → movimiento Receipt + actualiza WAC

### 1.5 CMP — Compras

- [x] Órdenes de Compra (cabecera + líneas)
- [x] Flujo de estados OC: Borrador → Confirmado → Aprobado → RecepciónParcial → Cerrado / Anulado
- [x] Recepciones de Compra (parcial / total)
- [x] Generación automática de movimiento inventario al confirmar recepción
- [x] Hojas de Importación / Landed Cost
- [x] Gastos de importación con prorrateo por: Valor / Unidades / Peso / Volumen
- [x] Ajuste de WAC al liquidar importación
- [x] Expedientes de importación

### 1.6 ACC — Contabilidad

- [x] Plan de Cuentas jerárquico multinivel (ej: 1.1.3.01)
- [x] Seed del plan de cuentas por defecto
- [x] Comprobantes contables: Ingreso (CI), Egreso (CE), Traspaso (CT)
- [x] Validación partida doble (Debe = Haber)
- [x] Tipo de cambio (USD, UFV) al momento del asiento
- [x] Tipo de pago: Efectivo, Cheque, QR, Transferencia, S/D
- [x] Estados del comprobante: Borrador → Contabilizado → Anulado
- [x] Copiar comprobante existente
- [x] Períodos contables mensuales (Apertura / Cierre)
- [x] Cierre de gestión anual con asientos de cierre y apertura — *PR #49 (S-07)*
- [x] Plantillas de contabilización automática por tipo de documento
- [x] Motor de contabilización automática
- [x] Notificaciones de períodos próximos a cerrar — *PR #47 (S-06)*
- [x] Balance General
- [x] Estado de Resultados
- [x] Sumas y Saldos (Balance de Comprobación)
- [x] Libro Diario
- [x] Libro Mayor
- [ ] 🔄 Flujo de Efectivo (estructura lista, UI parcial)

---

## FASE 2 — Ventas y Facturación (v1.1.0) 🔄 EN PROGRESO

### 2.1 VTA — Ventas
- [ ] 🔄 Pedidos de venta — cabecera (rama `feature/pedidos-venta`)
  - [x] Modelo de dominio `PedidoVenta` / `PedidoVentaLinea`
  - [x] Migración de BD (aplicada en Development; FK Almacenes ajustada)
  - [x] Servicio `IPedidoVentaService` (implementación básica)
  - [x] Controller `PedidosVentaController` (endpoints CRUD)
  - [x] DTO: Create / Update / PedidoDto
  - [x] HTTP Service en Web (`PedidoVentaHttpService`)
  - [x] Página Blazor: listado + formulario (básico)
  - [ ] Flujo de estados: Borrador → Confirmado → Despachado → Cerrado / Anulado
  - [ ] Validación de stock disponible al confirmar (pendiente integración con Inventario)
  - [ ] Generación de movimiento salida INV al despachar (pendiente)
- [ ] 🔄 Pedidos por sucursal (rama `PedidoSucursal`)
- [ ] Facturas de venta
  - [ ] Modelo `FacturaVenta` / `FacturaVentaLinea`
  - [ ] Migración de BD
  - [ ] CRUD completo (API + Web)
  - [ ] Generación desde pedido confirmado
  - [ ] Validación e impuesto (IT, IVA)
- [ ] Notas de Crédito y Débito
- [ ] Devoluciones de venta

### 2.2 CxC — Cuentas por Cobrar

- [ ] Entidad `CuentaPorCobrar`
- [ ] Vinculación Factura → CxC automática
- [ ] Registro de pagos parciales
- [ ] Reporte de cartera vencida
- [ ] UI: listado + registro de cobros

### 2.3 CxP — Cuentas por Pagar

- [ ] Entidad `CuentaPorPagar`
- [ ] Vinculación OC recepcionada → CxP automática
- [ ] Registro de pagos a proveedores
- [ ] Reporte de compromisos de pago
- [ ] UI: listado + registro de pagos

### 2.4 Facturación electrónica SIAT (Bolivia)

- [ ] Integración con servicio SIAT/SIN
- [ ] Emisión de facturas electrónicas (Modalidad Online)
- [ ] Anulación de facturas electrónicas
- [ ] Descarga de contingencia

---

## FASE 3 — Reportes y UX (v1.2.0) 🔲 PENDIENTE

### 3.1 Reportes PDF

- [ ] PDF Balance General
- [ ] PDF Estado de Resultados
- [ ] PDF Sumas y Saldos
- [ ] PDF Libro Diario
- [ ] PDF Libro Mayor
- [ ] PDF Flujo de Efectivo
- [ ] PDF Factura de Venta
- [ ] PDF Orden de Compra
- [ ] PDF Kardex por producto
- [ ] Integrar QuestPDF (ya instalado) en flujos

### 3.2 Exportación Excel en listados

- [ ] Clientes (lista completa)
- [ ] Proveedores
- [ ] Productos
- [ ] Stock por almacén
- [ ] Movimientos de inventario
- [ ] Órdenes de Compra
- [ ] Asientos contables
- [ ] Estados financieros

### 3.3 Dashboard con KPIs reales

- [ ] Stock bajo mínimo (alerta productos críticos)
- [ ] Movimientos del día (entradas/salidas)
- [ ] OC pendientes de aprobación
- [ ] Facturas vencidas (CxC)
- [ ] Pagos pendientes (CxP)
- [ ] Ingresos vs Egresos del mes (gráfico)
- [ ] Saldo contable por cuenta principal

### 3.4 UX y mejoras de interfaz

- [ ] UI para Industrias y Reglas por industria
- [ ] UI para Features activables (LOT, SERIAL, FEFO…)
- [ ] UI para Historial de versiones de entidades
- [ ] UI para Documentos multimedia de productos
- [ ] UI para Conciliación bancaria
- [ ] UI para Activos fijos y depreciación
- [ ] UI para Centros de costo
- [ ] UI para Ubicaciones de almacén
- [ ] UI para Presupuestos
- [ ] Paginación y filtros avanzados en todos los listados
- [ ] Búsqueda global desde la barra de navegación

---

## FASE 4 — Calidad y Operaciones (v1.2.0) 🔲 PENDIENTE

### 4.1 Tests automatizados

- [ ] Tests unitarios — Domain entities y lógica de negocio
- [ ] Tests unitarios — Application Services (mocks de repositorios)
- [ ] Tests de integración — API endpoints principales
- [ ] Tests de integración — Flujos completos (OC → Recepción → Stock)
- [ ] Cobertura mínima: 60% en Application layer
- [ ] Configurar xUnit + FluentAssertions + Moq

### 4.2 CI/CD — GitHub Actions

- [ ] Pipeline `build-and-test.yml` — compile + test en cada PR
- [ ] Pipeline `deploy-api.yml` — publicar API en servidor
- [ ] Pipeline `deploy-web.yml` — publicar Blazor WASM
- [ ] Secrets configurados (DB, JWT Key, URLs)
- [ ] Badge de estado en README

### 4.3 Seguridad y hardening

- [ ] Cambiar credenciales admin por defecto en producción
- [ ] Cambiar JWT Key en producción (actualmente hardcoded en appsettings)
- [ ] Habilitar HTTPS obligatorio en API
- [ ] Revisar CORS en producción (restringir a dominio real)
- [ ] Implementar rate limiting en endpoints de auth
- [ ] Validación de inputs con FluentValidation en DTOs Create/Update
- [ ] Revisar exposición de stack trace en errores

### 4.4 Performance

- [ ] Agregar índices en campos de búsqueda frecuente (NIT, Código, Sku)
- [ ] Paginación en todos los endpoints GET de listas (actualmente algunos retornan todo)
- [ ] Caché para datos de referencia (catálogos, UdM, monedas)
- [ ] Profiling de queries N+1 con EF Core logging

---

## FASE 5 — Escalabilidad y Futuro (v2.0.0+) 🔲 LARGO PLAZO

### 5.1 Contabilidad avanzada

- [ ] Costeo FIFO real (actualmente solo WAC)
- [ ] Costeo Estándar
- [ ] Diferencia de cambio automática (revalorización de saldos)
- [ ] Conciliación bancaria completa con extracción de extractos
- [ ] Activos fijos: compra, mejora, baja, depreciación mensual automática
- [ ] Presupuestos anuales con comparativo real vs presupuesto

### 5.2 Operaciones avanzadas

- [ ] Workflow engine configurable (tareas, aprobaciones)
- [ ] Notificaciones en tiempo real (SignalR)
- [ ] Planificación de necesidades de materiales (MRP básico)
- [ ] Control de calidad en recepciones

### 5.3 Multi-idioma y accesibilidad

- [ ] i18n (internacionalización) — español / inglés
- [ ] Temas claro/oscuro
- [ ] Accesibilidad WCAG 2.1

### 5.4 Infraestructura cloud

- [ ] Migración a Azure App Service (API)
- [ ] Azure SQL / SQL Managed Instance
- [ ] Azure Blob Storage para documentos
- [ ] Azure Application Insights (observabilidad)
- [ ] Multi-tenant con aislamiento por base de datos (un schema o BD por empresa)

### 5.5 Integraciones externas

- [ ] API pública documentada para integraciones de terceros
- [ ] Integración con entidades bancarias (extractos automáticos)
- [ ] Integración con plataformas e-commerce
- [ ] App móvil (MAUI / Blazor Hybrid)

---

## CHECKLIST OPERATIVO — Antes de ir a producción

### Configuración

- [ ] Cambiar `JwtSettings:Key` en `appsettings.json` por valor seguro
- [ ] Verificar `ConnectionStrings:DefaultConnection` apunta al servidor correcto
- [ ] Cambiar contraseña del usuario admin por defecto
- [ ] Configurar `BlazorBaseUrl` con la URL real del frontend
- [ ] Establecer `AllowedOrigins` en CORS con el dominio real
- [ ] Deshabilitar `StubAuthHandler` (solo debe estar en Development)
- [ ] Configurar `ASPNETCORE_ENVIRONMENT=Production`

### Base de datos

- [ ] Ejecutar `dotnet ef database update` en el servidor de producción
- [ ] Verificar que todos los seeds están aplicados (admin, catálogo, estados de producto)
- [ ] Configurar backup automático de la BD
- [ ] Verificar que el usuario SQL tiene permisos correctos (no sa en producción)

### Despliegue

- [ ] `dotnet publish` del API con configuración Release
- [ ] `dotnet publish` del Web con configuración Release
- [ ] Configurar proxy reverso (IIS / Nginx / Caddy)
- [ ] Verificar endpoint `/health` responde 200 Healthy
- [ ] Verificar Swagger deshabilitado en producción
- [ ] Configurar certificados SSL

### Validación funcional (Smoke test)

- [ ] Login con admin funciona
- [ ] Crear empresa de prueba
- [ ] Crear sucursal
- [ ] Crear usuario y asignar a empresa
- [ ] Crear producto y activarlo en empresa
- [ ] Registrar movimiento de inventario
- [ ] Crear OC, aprobar y recepcionar
- [ ] Verificar stock actualizado
- [ ] Crear asiento contable y contabilizar
- [ ] Generar Balance General

---

> **Mantenido por:** Abel Calvimontes  
> **Última actualización:** 11 de mayo 2026

# Plan de Implementación — Módulo de Ventas con Facturación
## AgoraHUB360 ERP

**Objetivo:** implementar el módulo de Ventas de forma incremental, sólida, multiempresa y escalable, separando Backend y Frontend para que GitHub Copilot trabaje paso a paso sin romper módulos existentes.

---

# 0. Principios No Negociables

## Arquitectura

- Mantener Clean Architecture.
- No mezclar lógica de negocio en Controllers ni en Razor.
- El Backend manda sobre reglas de negocio.
- El Frontend solo consume DTOs y muestra estados.
- No hacer cambios masivos.
- Compilar después de cada fase.

## Multiempresa

- `EmpresaId` siempre debe salir del JWT / `ICurrentUserService`.
- Nunca aceptar `EmpresaId` desde Frontend.
- Toda entidad tenant-aware debe validar pertenencia a empresa activa.
- Validar:
  - Cliente
  - Sucursal
  - Almacén
  - CompanyProduct
  - PedidoVenta origen
  - Servicios/conceptos si aplican

## Ventas

El módulo debe soportar:

1. Venta directa.
2. Venta desde PedidoVenta.
3. Venta de productos.
4. Venta de servicios.
5. Venta mixta: productos + servicios.
6. Facturación al mismo cliente.
7. Facturación a otro NIT / Razón Social.
8. Pago contado.
9. Pago crédito.
10. Pago mixto.
11. Diferentes modos de pago.
12. Integración futura con SIAT/SIN.
13. Integración futura con CxC.

---

# PARTE I — BACKEND

---

# Backend — Fase 1: Diagnóstico antes de modificar

## Objetivo

Antes de crear código, revisar qué ya existe en ventas, pedidos, inventario, facturación, pagos y DTOs.

## Prompt para GitHub Copilot

```text
Actúa como Senior .NET Architect experto en Clean Architecture, EF Core 8, SQL Server, ERP multiempresa y Blazor WebAssembly.

Contexto:
Estoy trabajando en AgoraHUB360 ERP.
Antes de implementar Ventas con Facturación, necesito un diagnóstico técnico del estado actual del backend.

Objetivo:
Analiza el código existente sin modificar nada.

Revisa estas áreas:
- Entidades Domain relacionadas con ventas.
- PedidoVenta y PedidoVentaDetalle.
- Inventario, StockProducto, MovimientoInventario, Kardex.
- Clientes y ClienteSucursal.
- Productos y CompanyProduct.
- Almacenes y Sucursales.
- NumeraciónDocumento.
- Contabilidad si existe integración.
- DTOs Shared relacionados.
- Controllers API relacionados.
- Servicios Application relacionados.
- DbContext y configuraciones EF.

Entrega:
1. Lista de archivos existentes relevantes.
2. Qué entidades ya existen.
3. Qué DTOs ya existen.
4. Qué servicios ya existen.
5. Qué controllers ya existen.
6. Qué endpoints ya existen.
7. Qué falta crear.
8. Qué no se debe tocar todavía.
9. Riesgos de romper módulos existentes.
10. Plan recomendado por fases.

Restricciones:
- No modificar código.
- No crear archivos.
- No aplicar migraciones.
- Solo diagnóstico.
```

## Resultado esperado

Documento/resumen dentro del chat de Copilot con el mapa real del código actual.

---

# Backend — Fase 2: Modelo de dominio de Venta

## Objetivo

Crear o ajustar entidades mínimas para soportar venta directa, venta desde pedido, productos, servicios, facturación y pagos.

## Entidades recomendadas

### `Venta`

Campos sugeridos:

- `Id`
- `EmpresaId`
- `SucursalId`
- `AlmacenId`
- `ClienteId`
- `PedidoVentaId`
- `NumeroVenta`
- `FechaVenta`
- `TipoVenta`
- `EstadoVenta`
- `EstadoPago`
- `MonedaId`
- `TipoCambio`
- `Subtotal`
- `DescuentoTotal`
- `ImpuestoTotal`
- `Total`
- `Observaciones`
- `InventarioDescontado`
- `FacturaGenerada`
- `Activo`

### `VentaDetalle`

Campos sugeridos:

- `Id`
- `VentaId`
- `TipoItemVenta`
- `CompanyProductId`
- `ServicioId` o `ConceptoServicioId`
- `AlmacenId`
- `Descripcion`
- `Cantidad`
- `UnidadMedidaId`
- `PrecioUnitario`
- `DescuentoPorcentaje`
- `DescuentoMonto`
- `ImpuestoMonto`
- `TotalLinea`
- `CostoUnitario`
- `DescuentaInventario`

### `VentaFacturacionDatos`

Campos sugeridos:

- `Id`
- `VentaId`
- `Facturar`
- `FacturarAlMismoCliente`
- `TipoDocumentoIdentidad`
- `NitFactura`
- `Complemento`
- `RazonSocialFactura`
- `EmailFactura`
- `TelefonoFactura`
- `EstadoFactura`
- `FacturaId`

### `VentaPago`

Campos sugeridos:

- `Id`
- `VentaId`
- `FechaPago`
- `TipoPago`
- `ModoPago`
- `CuentaCajaBancoId`
- `Monto`
- `MonedaId`
- `TipoCambio`
- `Referencia`
- `EstadoPago`

## Enums recomendados

- `TipoVenta`: Directa, DesdePedido.
- `EstadoVenta`: Borrador, Confirmada, Facturada, Despachada, Pagada, Anulada.
- `TipoItemVenta`: Producto, Servicio.
- `TipoPago`: Contado, Credito, Mixto.
- `ModoPago`: Efectivo, QR, Transferencia, Tarjeta, Cheque, Deposito, Otro.
- `EstadoPagoVenta`: Pendiente, Parcial, Pagado, Anulado.
- `EstadoFacturaVenta`: NoGenerada, Pendiente, Generada, Anulada.

## Prompt para GitHub Copilot

```text
Actúa como Senior .NET Architect.

Objetivo:
Crear el modelo de dominio mínimo para Ventas con Facturación dentro de AgoraHUB360 ERP.

Antes de crear:
- Verifica si ya existen entidades Venta, VentaDetalle, VentaPago o similares.
- Si existen, no dupliques.
- Ajusta de forma mínima.
- Si no existen, créalas.

Crear o ajustar en Domain:
- Venta
- VentaDetalle
- VentaFacturacionDatos
- VentaPago
- Enums necesarios:
  - TipoVenta
  - EstadoVenta
  - TipoItemVenta
  - TipoPago
  - ModoPago
  - EstadoPagoVenta
  - EstadoFacturaVenta

Reglas:
- Venta debe heredar de TenantEntity.
- VentaDetalle, VentaFacturacionDatos y VentaPago deben estar correctamente relacionadas.
- No aceptar EmpresaId desde DTO.
- Venta debe poder venir de PedidoVenta opcionalmente.
- VentaDetalle debe soportar producto o servicio.
- Producto usa CompanyProductId.
- Servicio no requiere almacén ni stock.
- Producto inventariable requiere almacén y puede descontar inventario.

No modificar servicios todavía.
No crear endpoints todavía.
No tocar frontend.
Compilar al final.
```

## Validación

```bash
dotnet build
```

---

# Backend — Fase 3: DbContext y migración

## Objetivo

Registrar entidades nuevas en EF Core y crear migración controlada.

## Prompt para GitHub Copilot

```text
Actúa como experto en EF Core 8 y SQL Server.

Objetivo:
Registrar las entidades de Ventas en AgoraDbContext y crear configuración EF Core limpia.

Tareas:
1. Agregar DbSet para:
   - Venta
   - VentaDetalle
   - VentaFacturacionDatos
   - VentaPago

2. Configurar relaciones:
   - Venta 1:N VentaDetalle
   - Venta 1:1 VentaFacturacionDatos
   - Venta 1:N VentaPago
   - Venta opcionalmente se relaciona con PedidoVenta
   - Venta se relaciona con Cliente
   - Venta se relaciona con Sucursal
   - Venta se relaciona con Almacen

3. Configurar precisión decimal:
   - Cantidad decimal(18,4)
   - Precios decimal(18,4)
   - Totales decimal(18,2)
   - TipoCambio decimal(18,6)

4. Respetar TenantEntity y filtros globales existentes.

5. Crear migración:
   Add-Migration AddVentasModule

6. No ejecutar Update-Database automáticamente si el proyecto no lo hace normalmente.
7. Compilar al final.

Restricciones:
- No tocar lógica de negocio.
- No tocar frontend.
- No tocar módulos existentes salvo relaciones estrictamente necesarias.
```

## Validación

```bash
dotnet build
dotnet ef migrations list --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

---

# Backend — Fase 4: DTOs Shared

## Objetivo

Crear contratos limpios para Frontend/API.

## DTOs recomendados

- `VentaDto`
- `VentaDetalleDto`
- `CrearVentaRequestDto`
- `ActualizarVentaRequestDto`
- `ConfirmarVentaRequestDto`
- `GenerarVentaDesdePedidoRequestDto`
- `VentaFacturacionDatosDto`
- `VentaPagoDto`
- `RegistrarPagoVentaRequestDto`
- `VentaResumenDto`

## Prompt para GitHub Copilot

```text
Actúa como Senior Backend Developer experto en DTOs compartidos para Blazor + Web API.

Objetivo:
Crear DTOs para el módulo de Ventas en AgoraHUB360.ERP.Shared.

Crear carpeta si no existe:
src/AgoraHub360.ERP.Shared/DTOs/Ventas

Crear DTOs:
- VentaDto
- VentaDetalleDto
- CrearVentaRequestDto
- ActualizarVentaRequestDto
- ConfirmarVentaRequestDto
- GenerarVentaDesdePedidoRequestDto
- VentaFacturacionDatosDto
- VentaPagoDto
- RegistrarPagoVentaRequestDto
- VentaResumenDto

Reglas:
- Ningún DTO debe recibir EmpresaId.
- CrearVentaRequestDto debe incluir:
  - SucursalId
  - AlmacenId opcional/global para cabecera
  - ClienteId opcional
  - PedidoVentaId opcional
  - TipoVenta
  - FechaVenta
  - MonedaId
  - TipoCambio
  - Detalles
  - DatosFacturacion
  - Pagos opcionales
  - Observaciones

- VentaDetalleDto debe soportar:
  - TipoItemVenta
  - CompanyProductId opcional
  - ServicioId o ConceptoServicioId opcional
  - AlmacenId opcional
  - Descripcion
  - Cantidad
  - PrecioUnitario
  - Descuento
  - Impuesto
  - TotalLinea

- VentaFacturacionDatosDto debe permitir:
  - Facturar
  - FacturarAlMismoCliente
  - TipoDocumentoIdentidad
  - NitFactura
  - Complemento
  - RazonSocialFactura
  - EmailFactura
  - TelefonoFactura

- VentaPagoDto debe permitir:
  - TipoPago
  - ModoPago
  - Monto
  - CuentaCajaBancoId opcional
  - Referencia

Compilar al final.
No tocar servicios todavía.
```

---

# Backend — Fase 5: Servicio de aplicación VentaService

## Objetivo

Implementar reglas de negocio principales.

## Métodos mínimos

- `GetAllAsync`
- `GetByIdAsync`
- `CreateAsync`
- `UpdateAsync`
- `ConfirmarAsync`
- `CrearDesdePedidoAsync`
- `RegistrarPagoAsync`
- `AnularAsync`

## Prompt para GitHub Copilot

```text
Actúa como Senior Application Architect experto en ERP, ventas, inventario y multiempresa.

Objetivo:
Implementar VentaService en la capa Application.

Primero verifica patrones existentes:
- Result pattern usado en el proyecto.
- Repositorios usados.
- Servicios similares como PedidoVentaService, OrdenCompraService, MovimientoInventarioService, ClienteService.

Crear o ajustar:
- IVentaService
- VentaService

Métodos mínimos:
- GetAllAsync
- GetByIdAsync
- CreateAsync
- UpdateAsync
- ConfirmarAsync
- CrearDesdePedidoAsync
- RegistrarPagoAsync
- AnularAsync

Reglas multiempresa:
- Obtener empresa activa desde ICurrentUserService.
- Si no hay EmpresaId en JWT, retornar error claro.
- No aceptar EmpresaId del DTO.
- Validar que SucursalId pertenece a empresa activa.
- Validar que AlmacenId pertenece a empresa activa.
- Validar que ClienteId pertenece a empresa activa si viene informado.
- Validar que PedidoVentaId pertenece a empresa activa si viene informado.
- Validar que cada CompanyProductId pertenece a empresa activa.
- Validar que AlmacenId de cada producto pertenece a empresa activa y a la sucursal si aplica.

Reglas de detalle:
- Si TipoItemVenta = Producto:
  - CompanyProductId obligatorio.
  - AlmacenId obligatorio si descuenta inventario.
  - Validar stock disponible al confirmar.
- Si TipoItemVenta = Servicio:
  - No requiere CompanyProductId.
  - No requiere AlmacenId.
  - No descuenta inventario.

Reglas de facturación:
- Permitir Facturar = false.
- Si Facturar = true:
  - NitFactura obligatorio.
  - RazonSocialFactura obligatorio.
- Permitir facturar al mismo cliente o a otro NIT/Razón Social.

Reglas de pagos:
- Permitir venta sin pago.
- Permitir pago parcial.
- Permitir pago total.
- Calcular EstadoPago:
  - Pendiente
  - Parcial
  - Pagado
- Validar que pagos no excedan total salvo que el estándar del proyecto permita saldo a favor.

Reglas de inventario:
- En Borrador no descontar inventario.
- En ConfirmarAsync validar stock.
- Descontar inventario solo si la regla actual del proyecto ya descuenta al confirmar.
- Si el proyecto descuenta al despachar, dejar ConfirmarAsync solo validando/reservando.
- No duplicar descuento.
- Usar MovimientoInventarioService si existe.
- No implementar inventario paralelo.

Reglas desde Pedido:
- CrearDesdePedidoAsync debe cargar datos del pedido confirmado.
- No permitir convertir un pedido anulado.
- No permitir convertir dos veces el mismo pedido si ya existe venta activa asociada.
- Respetar reservas si el pedido ya las aplicó.

Restricciones:
- No tocar frontend.
- No crear controller todavía si no es necesario.
- No romper PedidoVentaService.
- No duplicar lógica de inventario.
- Compilar al final.
```

---

# Backend — Fase 6: API Controller

## Objetivo

Exponer endpoints REST versionados.

## Endpoints sugeridos

- `GET /api/v1/ventas`
- `GET /api/v1/ventas/{id}`
- `POST /api/v1/ventas`
- `PUT /api/v1/ventas/{id}`
- `POST /api/v1/ventas/{id}/confirmar`
- `POST /api/v1/ventas/desde-pedido`
- `POST /api/v1/ventas/{id}/pagos`
- `POST /api/v1/ventas/{id}/anular`

## Prompt para GitHub Copilot

```text
Actúa como Senior ASP.NET Core Web API Developer.

Objetivo:
Crear el controller REST para Ventas.

Crear:
src/AgoraHub360.ERP.Api/Controllers/V1/VentasController.cs

Endpoints:
- GET /api/v1/ventas
- GET /api/v1/ventas/{id}
- POST /api/v1/ventas
- PUT /api/v1/ventas/{id}
- POST /api/v1/ventas/{id}/confirmar
- POST /api/v1/ventas/desde-pedido
- POST /api/v1/ventas/{id}/pagos
- POST /api/v1/ventas/{id}/anular

Reglas:
- Usar [Authorize].
- Mantener ApiController y Route del estándar del proyecto.
- Usar IVentaService.
- No poner lógica de negocio en controller.
- Respetar Result pattern existente.
- Devolver respuestas consistentes con otros controllers.
- No exponer EmpresaId.
- Validar id de ruta vs DTO si aplica.

Compilar al final.
```

---

# Backend — Fase 7: Registro DI

## Objetivo

Registrar servicios en contenedor.

## Prompt para GitHub Copilot

```text
Actúa como experto en configuración ASP.NET Core y Clean Architecture.

Objetivo:
Registrar IVentaService y dependencias necesarias en DI.

Tareas:
- Buscar dónde se registran servicios Application.
- Registrar IVentaService -> VentaService.
- Verificar si los repositorios genéricos ya cubren Venta, VentaDetalle, VentaPago y VentaFacturacionDatos.
- No duplicar registros existentes.
- Compilar al final.

Restricciones:
- No modificar lógica de negocio.
- No tocar frontend.
```

---

# Backend — Fase 8: Pruebas manuales API

## Objetivo

Validar flujo mínimo por Swagger/Postman.

## Casos mínimos

1. Crear venta directa en borrador.
2. Crear venta directa con producto.
3. Crear venta directa con servicio.
4. Crear venta mixta producto + servicio.
5. Confirmar venta.
6. Registrar pago parcial.
7. Registrar pago total.
8. Crear venta con datos de facturación a otro NIT.
9. Crear venta desde pedido.
10. Intentar convertir pedido ya convertido.
11. Intentar vender producto de otra empresa.
12. Intentar vender desde almacén de otra empresa.
13. Intentar venta sin stock.

## Prompt para GitHub Copilot

```text
Actúa como QA Técnico para API ERP.

Objetivo:
Generar una checklist de pruebas manuales para validar el módulo de Ventas desde Swagger o Postman.

Necesito:
- Casos felices.
- Casos de error.
- Casos multiempresa.
- Casos de producto.
- Casos de servicio.
- Casos de facturación.
- Casos de pago.
- Casos desde pedido.

Para cada caso indica:
- Endpoint.
- Payload ejemplo.
- Resultado esperado.
- Validación en base de datos.
- Riesgo que cubre.

No modifiques código.
```

---

# PARTE II — FRONTEND

---

# Frontend — Fase 1: Diagnóstico UI actual

## Objetivo

Revisar páginas, servicios HTTP y rutas existentes de Ventas/Pedidos.

## Prompt para GitHub Copilot

```text
Actúa como Senior Blazor WebAssembly Developer.

Contexto:
AgoraHUB360 ERP usa Blazor WebAssembly y consume API REST.

Objetivo:
Analizar el frontend actual antes de implementar pantalla de Ventas con Factura.

Revisar:
- Pages relacionadas con ventas.
- Pages relacionadas con pedidos.
- Services HTTP existentes.
- DTOs usados.
- Menú/layout/rutas.
- Componentes reutilizables.
- Estilos existentes.

Entrega:
1. Archivos actuales relacionados.
2. Servicios HTTP existentes.
3. Rutas actuales.
4. Qué página se puede reutilizar.
5. Qué debe crearse nuevo.
6. Riesgos de romper UI actual.
7. Plan incremental recomendado.

Restricciones:
- No modificar código.
- No crear archivos.
```

---

# Frontend — Fase 2: Servicio HTTP de Ventas

## Objetivo

Crear cliente HTTP para endpoints backend.

## Prompt para GitHub Copilot

```text
Actúa como Senior Blazor WASM Developer experto en consumo de Web API.

Objetivo:
Crear o ajustar el servicio HTTP de Ventas.

Crear:
src/AgoraHub360.ERP.Web/Services/Ventas/VentaHttpService.cs
o usar la carpeta/patrón existente del proyecto.

Métodos:
- GetAllAsync
- GetByIdAsync
- CreateAsync
- UpdateAsync
- ConfirmarAsync
- CrearDesdePedidoAsync
- RegistrarPagoAsync
- AnularAsync

Reglas:
- Usar ApiResponse o patrón existente del proyecto.
- Usar DTOs de AgoraHub360.ERP.Shared.DTOs.Ventas.
- BaseUrl: api/v1/ventas
- No enviar EmpresaId.
- Manejar errores de API de forma clara.
- Registrar en DI si el patrón lo requiere.

No crear páginas todavía.
Compilar al final.
```

---

# Frontend — Fase 3: Página listado de ventas

## Objetivo

Crear listado operativo.

## Campos en listado

- Número
- Fecha
- Cliente
- Tipo venta
- Estado
- Estado pago
- Total
- Factura
- Acciones

## Prompt para GitHub Copilot

```text
Actúa como experto en Blazor WebAssembly y UX ERP.

Objetivo:
Crear la página de listado de ventas.

Crear o ajustar:
src/AgoraHub360.ERP.Web/Pages/Ventas/Ventas.razor

Funcionalidades:
- Cargar ventas desde VentaHttpService.GetAllAsync.
- Mostrar tabla con:
  - NumeroVenta
  - FechaVenta
  - Cliente
  - TipoVenta
  - EstadoVenta
  - EstadoPago
  - Total
  - EstadoFactura
- Botón Nueva Venta.
- Botón Ver/Editar.
- Botón Confirmar si está en Borrador.
- Botón Anular si aplica.
- Mensajes de carga, vacío y error.
- No usar datos hardcodeados.
- No romper rutas existentes.

Agregar ruta:
@page "/ventas"

Si existe otra ruta, respetar el estándar del proyecto.
Compilar al final.
```

---

# Frontend — Fase 4: Página venta con factura

## Objetivo

Implementar pantalla principal basada en el prototipo HTML.

## Secciones

1. Datos de venta.
2. Cliente.
3. Productos y servicios.
4. Facturación.
5. Pagos.
6. Resumen.
7. Control operativo.

## Prompt para GitHub Copilot

```text
Actúa como Senior Blazor WebAssembly Developer experto en formularios ERP.

Objetivo:
Crear una página de venta con factura basada en el prototipo HTML aprobado.

Crear o ajustar:
src/AgoraHub360.ERP.Web/Pages/Ventas/VentaForm.razor

Ruta sugerida:
@page "/ventas/nueva"
@page "/ventas/{Id:int}"

Secciones:
1. Datos de la venta:
   - TipoVenta: Directa / DesdePedido
   - Pedido origen opcional
   - Sucursal
   - Almacén
   - Fecha
   - Moneda
   - TipoCambio
   - Vendedor actual solo lectura si aplica

2. Cliente comercial:
   - Buscar cliente
   - Seleccionar cliente
   - Registrar cliente rápido futuro como placeholder/botón

3. Productos y servicios:
   - Agregar producto
   - Agregar servicio
   - Cantidad
   - Precio
   - Descuento
   - Impuesto
   - Total línea
   - Producto requiere CompanyProductId y AlmacenId
   - Servicio no requiere almacén

4. Facturación:
   - Facturar sí/no
   - Facturar al mismo cliente sí/no
   - TipoDocumentoIdentidad
   - NitFactura
   - Complemento
   - RazonSocialFactura
   - EmailFactura
   - TelefonoFactura

5. Pagos:
   - TipoPago
   - ModoPago
   - Monto
   - Referencia
   - CuentaCajaBancoId opcional
   - Permitir múltiples pagos

6. Resumen:
   - Subtotal productos
   - Subtotal servicios
   - Descuento
   - Impuesto
   - Total
   - Pagado
   - Saldo

Acciones:
- Guardar borrador
- Confirmar
- Confirmar + Facturar como botón visual, aunque backend inicialmente solo confirme si todavía no existe factura real.
- Cancelar

Reglas:
- No enviar EmpresaId.
- Validar datos mínimos antes de enviar.
- Calcular totales en frontend solo para UX; backend recalcula y valida.
- Mostrar errores del backend.
- No usar datos hardcodeados salvo placeholders visuales controlados.
- Usar DTOs Shared.
- Compilar al final.
```

---

# Frontend — Fase 5: Catálogos necesarios

## Objetivo

Conectar combos reales.

## Catálogos

- Sucursales del tenant actual.
- Almacenes por sucursal.
- Clientes.
- Productos habilitados `CompanyProduct`.
- Servicios/conceptos.
- Monedas.
- Cuentas caja/banco si existe.

## Prompt para GitHub Copilot

```text
Actúa como Senior Frontend Architect para ERP multiempresa.

Objetivo:
Conectar la página VentaForm.razor con catálogos reales existentes.

Revisar servicios HTTP existentes para:
- Sucursales
- Almacenes
- Clientes
- CompanyProducts
- Servicios o conceptos
- Monedas
- Cajas/Bancos

Tareas:
1. Cargar sucursales del tenant actual.
2. Al cambiar sucursal, cargar almacenes de esa sucursal.
3. Cargar clientes del tenant actual.
4. Cargar productos habilitados para venta desde CompanyProduct.
5. Si no existe catálogo de servicios, dejar estructura preparada con lista temporal controlada o TODO claro, sin romper compilación.
6. Cargar monedas si existe servicio.
7. Cargar cuentas de caja/banco si existe servicio.

Reglas:
- No consumir endpoints globales inseguros.
- No enviar EmpresaId.
- No filtrar empresa solo en cliente si existe endpoint scoped.
- Mostrar mensajes claros si faltan catálogos.
- Compilar al final.
```

---

# Frontend — Fase 6: Flujo desde pedido

## Objetivo

Permitir crear venta desde PedidoVenta.

## Prompt para GitHub Copilot

```text
Actúa como Senior Blazor Developer experto en flujos transaccionales ERP.

Objetivo:
Agregar flujo para crear venta desde PedidoVenta.

Tareas:
1. En listado de pedidos o detalle de pedido, agregar botón:
   "Generar venta"
   solo si el pedido está confirmado y no tiene venta asociada.

2. Al hacer clic:
   - Navegar a /ventas/nueva?pedidoId={id}
   o usar ruta equivalente.

3. En VentaForm.razor:
   - Detectar pedidoId.
   - Llamar al backend para generar/cargar venta desde pedido según endpoint disponible.
   - Precargar:
     - Cliente
     - Sucursal
     - Almacén
     - Productos
     - Cantidades
     - Precios
     - Totales

4. Respetar reservas de pedido si el backend lo maneja.
5. No permitir editar datos críticos si el pedido ya está confirmado, salvo que backend lo permita.

Reglas:
- No duplicar venta desde el mismo pedido.
- Mostrar error claro si el pedido ya fue convertido.
- Compilar al final.
```

---

# Frontend — Fase 7: Validación visual y UX

## Objetivo

Dejar pantalla clara y segura para usuario.

## Prompt para GitHub Copilot

```text
Actúa como experto UX/UI para sistemas ERP empresariales en Blazor.

Objetivo:
Mejorar la experiencia visual de VentaForm.razor y Ventas.razor sin cambiar reglas de negocio.

Mejoras:
- Estados con badges.
- Diferenciar Producto y Servicio.
- Resumen siempre visible.
- Botones claros:
  - Guardar borrador
  - Confirmar
  - Confirmar + Facturar
  - Registrar pago
  - Anular
- Mensajes de error visibles.
- Indicador de loading.
- Confirmación antes de anular.
- Diseño responsive.
- Mantener estilo visual del proyecto.

Restricciones:
- No modificar backend.
- No cambiar DTOs.
- No romper compilación.
```

---

# PARTE III — INTEGRACIÓN Y CONTROL

---

# Fase Final: Validación End-to-End

## Flujo mínimo esperado

1. Login.
2. Seleccionar empresa activa.
3. Ir a Ventas.
4. Crear venta directa.
5. Agregar producto.
6. Agregar servicio.
7. Completar datos de facturación.
8. Registrar pago parcial.
9. Guardar borrador.
10. Confirmar venta.
11. Ver estado actualizado.
12. Registrar pago final.
13. Validar inventario.
14. Validar que no se duplique descuento.
15. Crear venta desde pedido confirmado.

## Prompt para GitHub Copilot

```text
Actúa como QA Lead y Arquitecto de Software.

Objetivo:
Validar el módulo de Ventas end-to-end.

Revisa:
- Backend compila.
- Frontend compila.
- Migraciones correctas.
- Endpoints accesibles.
- DTOs alineados.
- Venta directa funciona.
- Venta desde pedido funciona.
- Producto descuenta o reserva inventario según regla existente.
- Servicio no toca inventario.
- Facturación guarda NIT y Razón Social.
- Pagos actualizan estado.
- Multiempresa está protegida.
- No se acepta EmpresaId desde frontend.
- No hay datos hardcodeados.
- No hay doble descuento de inventario.
- No hay doble conversión de pedido a venta.

Entrega:
1. Checklist de validación.
2. Errores encontrados.
3. Parches mínimos sugeridos.
4. Qué dejar para fase posterior.
```

---

# Recomendación de Ejecución

## Orden seguro

1. Diagnóstico Backend.
2. Modelo Domain.
3. DbContext + migración.
4. DTOs.
5. VentaService.
6. Controller.
7. DI.
8. Pruebas Swagger.
9. Diagnóstico Frontend.
10. VentaHttpService.
11. Listado Ventas.
12. Formulario Venta con Factura.
13. Catálogos reales.
14. Flujo desde Pedido.
15. UX final.
16. Validación End-to-End.

## Qué NO implementar en esta primera iteración

Para no romper el sistema, dejar para fase posterior:

- SIAT/SIN real.
- CxC completa.
- Contabilización automática definitiva.
- Notas de crédito/débito.
- Devoluciones.
- Promociones avanzadas.
- Comisiones.
- Descuentos por listas complejas.
- Workflow de aprobación.
- Integración BI.

---

# Prompt Maestro para Iniciar Cada Sesión con Copilot

```text
Actúa como Senior .NET Architect + Senior Blazor Developer + MBA funcional ERP.

Proyecto:
AgoraHUB360 ERP.

Stack:
.NET 8, C# 12, Blazor WebAssembly, ASP.NET Core Web API, EF Core 8, SQL Server, Clean Architecture.

Reglas obligatorias:
- No hacer cambios masivos.
- Trabajar por fases.
- Compilar después de cada fase.
- No aceptar EmpresaId desde frontend.
- Usar EmpresaId desde JWT / ICurrentUserService.
- Mantener tenant isolation.
- No romper Core, MDM, Inventario, Compras ni Contabilidad.
- No duplicar lógica existente.
- Revisar antes de crear.
- Si existe algo, reutilizar.
- Si falta algo, crear mínimo viable.
- Entregar cambios por archivos.

Módulo a implementar:
Ventas con:
- Venta directa.
- Venta desde pedido.
- Productos.
- Servicios.
- Facturación al mismo cliente u otro NIT/Razón Social.
- Pagos contado/crédito/mixto.
- Modos de pago.
- Integración futura con SIAT, CxC, Contabilidad y BI.

Antes de modificar cualquier archivo:
1. Analiza lo existente.
2. Enumera archivos a tocar.
3. Explica riesgo.
4. Propón patch mínimo.
5. Aplica cambios.
6. Compila.
```

---

# Cierre Ejecutivo

Este plan permite construir Ventas como un módulo empresarial real, no solo como una pantalla.  
La separación Backend/Frontend reduce riesgos, respeta la arquitectura actual y deja el sistema preparado para facturación electrónica, cuentas por cobrar, contabilidad automática y BI.

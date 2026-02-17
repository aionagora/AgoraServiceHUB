# 📊 Configuración de Tablero de Proyecto - AgoraHUB360 ERP

Esta guía explica cómo configurar y usar el tablero de proyecto con la jerarquía: **Epics → Features → Tasks**

## 🎯 Estructura del Tablero

```
Epic (Iniciativa grande)
  ├── Feature 1 (Funcionalidad específica)
  │   ├── Task 1.1 (Tarea técnica)
  │   ├── Task 1.2 (Tarea técnica)
  │   └── Task 1.3 (Tarea técnica)
  │
  ├── Feature 2 (Funcionalidad específica)
  │   ├── Task 2.1 (Tarea técnica)
  │   └── Task 2.2 (Tarea técnica)
  │
  └── Feature 3 (Funcionalidad específica)
      └── Task 3.1 (Tarea técnica)
```

## 📋 Niveles de Trabajo

### 1️⃣ Epic (Épica)
**Definición:** Iniciativa grande que abarca múltiples features y puede durar varios sprints.

**Características:**
- Duración: 1-3 meses
- Tamaño: Demasiado grande para completar en un sprint
- Valor: Alto impacto en el negocio
- Compuesta por: Múltiples features

**Ejemplos:**
- "Sistema de Ventas Completo"
- "Módulo de Inventario Valorizado"
- "Portal de Cliente B2B"
- "Integración con Sistemas Externos"

**Template de Epic:**
```markdown
## 🎯 Objetivo
[Describe el objetivo de alto nivel]

## 💼 Valor de Negocio
[Explica el valor que aporta al negocio]

## 📊 Métricas de Éxito
- [ ] Métrica 1
- [ ] Métrica 2
- [ ] Métrica 3

## 🔗 Features Relacionadas
- #feature-1
- #feature-2
- #feature-3

## 🗓️ Timeline
- Inicio estimado: [Fecha]
- Fin estimado: [Fecha]

## 👥 Stakeholders
- Product Owner: [@usuario]
- Tech Lead: [@usuario]
- Team: [@equipo]
```

### 2️⃣ Feature (Funcionalidad)
**Definición:** Funcionalidad específica que aporta valor al usuario y puede completarse en 1-2 sprints.

**Características:**
- Duración: 1-2 semanas
- Tamaño: Completable en un sprint
- Valor: Funcionalidad entregable al usuario
- Compuesta por: Múltiples tasks

**Ejemplos:**
- "CRUD de Cotizaciones"
- "Validación de Stock en Ventas"
- "Reporte de Kardex Valorizado"
- "API REST de Productos"

**Template de Feature:**
```markdown
## 📝 Descripción
[Describe la funcionalidad en detalle]

## 👤 User Story
Como [rol]
Quiero [funcionalidad]
Para [beneficio]

## ✅ Criterios de Aceptación
- [ ] Criterio 1
- [ ] Criterio 2
- [ ] Criterio 3

## 🔗 Epic
Parte de: #epic-123

## 📋 Tasks
- [ ] #task-1
- [ ] #task-2
- [ ] #task-3

## 🎨 Diseño/Mockups
[Enlaces a diseños si aplica]

## 🧪 Plan de Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] E2E tests

## 📅 Estimación
- Story Points: [puntos]
- Tiempo estimado: [horas/días]

## 🏷️ Labels
`feature` `[módulo]` `[prioridad]`
```

### 3️⃣ Task (Tarea)
**Definición:** Tarea técnica específica y accionable que puede completarse en menos de 1 día.

**Características:**
- Duración: 2-8 horas
- Tamaño: Completable en medio día a 1 día
- Valor: Trabajo técnico específico
- Asignable a: Un desarrollador

**Ejemplos:**
- "Crear migración de tabla cotizaciones"
- "Implementar controller de CotizacionesController"
- "Crear componente Vue para formulario de cotización"
- "Escribir tests unitarios de validación de stock"

**Template de Task:**
```markdown
## 🎯 Objetivo
[Describe qué se debe hacer]

## 🔗 Feature
Parte de: #feature-456

## ✅ Definición de Hecho
- [ ] Código implementado
- [ ] Tests escritos y pasando
- [ ] Code review aprobado
- [ ] Documentación actualizada

## 🛠️ Detalles Técnicos
[Información técnica relevante]

## 📦 Dependencias
- [ ] #task-x debe completarse antes
- [ ] Requiere acceso a [recurso]

## ⏱️ Estimación
- Tiempo estimado: [horas]

## 🏷️ Labels
`task` `[tipo]` `[módulo]`
```

## 🏗️ Configuración del Tablero GitHub Projects

### Paso 1: Crear Proyecto
1. Ve a tu repositorio en GitHub
2. Click en "Projects" → "New project"
3. Selecciona "Board" template
4. Nombra: "AgoraHUB360 - Desarrollo"

### Paso 2: Configurar Columnas
Crea las siguientes columnas:

| Columna | Propósito |
|---------|-----------|
| 📋 Backlog | Issues sin priorizar |
| 🎯 To Do | Priorizadas para trabajar |
| 🏗️ In Progress | En desarrollo activo |
| 👀 In Review | En code review |
| 🧪 Testing | En QA/Testing |
| ✅ Done | Completadas |

### Paso 3: Configurar Campos Personalizados

#### Campo: Type (Tipo)
- Epic
- Feature
- Task

#### Campo: Priority (Prioridad)
- 🔴 Critical
- 🟠 High
- 🟡 Medium
- 🟢 Low

#### Campo: Module (Módulo)
- Ventas
- Compras
- Inventario
- Contabilidad
- Finanzas
- CRM
- RRHH
- Auth
- Core

#### Campo: Sprint
- Sprint 1
- Sprint 2
- (etc.)

#### Campo: Story Points
- Número: 1, 2, 3, 5, 8, 13, 21

### Paso 4: Configurar Vistas

#### Vista 1: Por Tipo
```
Filtros:
- Agrupado por: Type
- Ordenado por: Priority
```

#### Vista 2: Por Módulo
```
Filtros:
- Agrupado por: Module
- Ordenado por: Priority
```

#### Vista 3: Sprint Actual
```
Filtros:
- Sprint: Sprint Actual
- Estado: No Done
- Agrupado por: Status
```

#### Vista 4: Epics Overview
```
Filtros:
- Type: Epic
- Mostrar: Todas
```

## 🏷️ Sistema de Labels

### Por Tipo
```
epic          # 🎯 Épica
feature       # ✨ Funcionalidad
task          # 📋 Tarea
bug           # 🐛 Error
hotfix        # 🚨 Corrección urgente
```

### Por Prioridad
```
priority: critical   # 🔴 Crítico
priority: high       # 🟠 Alto
priority: medium     # 🟡 Medio
priority: low        # 🟢 Bajo
```

### Por Módulo
```
module: ventas
module: compras
module: inventario
module: contabilidad
module: finanzas
module: crm
module: rrhh
module: auth
module: core
```

### Por Estado
```
status: blocked      # ⛔ Bloqueado
status: needs-info   # ❓ Necesita información
status: needs-review # 👀 Necesita revisión
status: ready        # ✅ Listo
```

### Por Tipo de Trabajo
```
type: backend       # Backend
type: frontend      # Frontend
type: database      # Base de datos
type: api           # API
type: documentation # Documentación
type: testing       # Testing
```

## 📝 Flujo de Trabajo

### Creación de Epic
1. Crear issue con template de Epic
2. Asignar label: `epic`, `[módulo]`, `[prioridad]`
3. Agregar al proyecto
4. Asignar campo "Type: Epic"
5. Vincular features relacionadas

### Creación de Feature
1. Crear issue con template de Feature
2. Asignar label: `feature`, `[módulo]`, `[prioridad]`
3. Agregar al proyecto
4. Asignar campo "Type: Feature"
5. Referenciar Epic padre: "Parte de #epic-123"
6. Crear tasks hijas

### Creación de Task
1. Crear issue con template de Task
2. Asignar label: `task`, `[tipo]`, `[módulo]`
3. Agregar al proyecto
4. Asignar campo "Type: Task"
5. Referenciar Feature padre: "Parte de #feature-456"
6. Asignar a desarrollador
7. Estimar story points

### Durante el Desarrollo
```
Backlog → To Do → In Progress → In Review → Testing → Done
```

1. **To Do:** Mover cuando se prioriza
2. **In Progress:** Mover al empezar desarrollo
3. **In Review:** Mover al crear PR
4. **Testing:** Mover al pasar code review
5. **Done:** Mover al mergear y cerrar issue

## 🎯 Ejemplo Práctico

### Epic: Sistema de Ventas
```markdown
Epic #100: Sistema de Ventas Completo
Labels: epic, module: ventas, priority: high

## Features relacionadas:
- [ ] #101 CRUD de Cotizaciones
- [ ] #102 Generación de Facturas
- [ ] #103 Gestión de Pedidos
```

### Feature: CRUD de Cotizaciones
```markdown
Feature #101: CRUD de Cotizaciones
Labels: feature, module: ventas, priority: high
Parte de: #100 (Epic: Sistema de Ventas)

## Tasks relacionadas:
- [ ] #110 Crear migración de tabla cotizaciones
- [ ] #111 Implementar modelo Cotizacion
- [ ] #112 Crear controller CotizacionController
- [ ] #113 Implementar vistas de listado
- [ ] #114 Implementar formulario de creación
- [ ] #115 Agregar validación de stock
- [ ] #116 Implementar generación de PDF
- [ ] #117 Escribir tests unitarios
```

### Tasks
```markdown
Task #110: Crear migración de tabla cotizaciones
Labels: task, type: database, module: ventas
Parte de: #101 (Feature: CRUD de Cotizaciones)
Asignado: @developer1
Story Points: 2

---

Task #111: Implementar modelo Cotizacion
Labels: task, type: backend, module: ventas
Parte de: #101 (Feature: CRUD de Cotizaciones)
Asignado: @developer1
Story Points: 3
```

## 📊 Métricas y Reportes

### Velocity Chart
```
Sprint 1: 25 points
Sprint 2: 30 points
Sprint 3: 28 points
Promedio: 27.6 points
```

### Burndown Chart
- Monitorear progreso del sprint
- Ajustar si está fuera de track

### Lead Time
- Tiempo promedio desde "To Do" hasta "Done"

### Cycle Time
- Tiempo promedio desde "In Progress" hasta "Done"

## 🔗 Automatizaciones

### Auto-mover al crear PR
```yaml
# .github/workflows/project-automation.yml
name: Project Automation

on:
  pull_request:
    types: [opened]

jobs:
  move-to-review:
    runs-on: ubuntu-latest
    steps:
      - name: Move to In Review
        uses: actions/github-script@v6
        with:
          script: |
            # Mover issue vinculada a columna "In Review"
```

### Auto-cerrar al mergear
```yaml
on:
  pull_request:
    types: [closed]

jobs:
  move-to-done:
    if: github.event.pull_request.merged == true
    runs-on: ubuntu-latest
    steps:
      - name: Move to Done
        # Mover a columna Done y cerrar issue
```

## 📚 Referencias

- [GitHub Projects Documentation](https://docs.github.com/en/issues/planning-and-tracking-with-projects)
- [Agile Epic vs Feature vs Story](https://www.atlassian.com/agile/project-management/epics-stories-themes)
- [GitHub Issues Best Practices](https://docs.github.com/en/issues/tracking-your-work-with-issues/about-issues)

---

**Mantenido por:** Equipo AgoraHUB360 ERP
**Última actualización:** 2026-02-17 05:48:02

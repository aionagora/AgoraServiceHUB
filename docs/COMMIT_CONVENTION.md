# Convención de Commits

## Tipos de Commits
Los tipos de commits que se pueden utilizar son:
- **feat**: una nueva característica
- **fix**: corrección de errores
- **docs**: documentación
- **style**: cambios que no afectan el significado del código (espacios en blanco, formato, etc.)
- **refactor**: cambios en el código que neither corrigen un bug ni añaden una característica
- **perf**: cambios que mejoran el rendimiento
- **test**: añadir tests faltantes o corregir tests existentes
- **build**: cambios que afectan el sistema de construcción o dependencias externas
- **ci**: cambios en los archivos y scripts de configuración de integración continua
- **chore**: tareas menores que no encajan en cualquiera de los tipos anteriores
- **revert**: volver a un commit anterior
- **migration**: cambios relacionados con la migración de bases de datos
- **seed**: cambios relacionados con la inicialización de la base de datos
- **security**: cambios relacionados con la seguridad
- **api**: cambios que afectan la API

## Ámbitos
Los ámbitos son opcionales y pueden ser utilizados para especificar qué parte del proyecto se afecta:
- **ventas**
- **compras**
- **inventario**
- **contabilidad**
- **finanzas**
- **crm**
- **rrhh**
- **auth**
- **core**
- **api**
- **ui**
- **config**
- **database**
- **tests**
- **docs**

## Reglas del Sujeto
El sujeto debe ser breve, pero informativo. Se recomienda el uso de un verbo en tiempo presente y de usar la capitalización solo en la primera letra (por ejemplo, "feat: añadir función de búsqueda").

## Formato del Cuerpo
El cuerpo debe incluir:
- La razón del cambio
- El efecto del cambio

## Tipos de Pie de Página
Los pies de página pueden incluir:
- **BREAKING CHANGE:** Cuando un cambio introduce una ruptura en la API que requiere atención especial.

## Ejemplos de Commits
- **Características:**  feat(ventas): añadir carrito de compra
- **Corrección de errores:** fix(compras): corregir error en cálculo
- **Refactor:** refactor(inventario): optimizar algoritmo de búsqueda
- **Mejoras de rendimiento:** perf(api): reducir tiempo de respuesta
- **Cambios de seguridad:** security(auth): actualizar dependencias para corregir vulnerabilidades

## Configuración de Commitlint
Ejemplo de archivo de configuración:
```json
{
  "extends": [
    "@commitlint/config-conventional"
  ]
}
```

## Generación de Changelog
Utilizar **standard-version** para generar changelogs automáticamente basado en los commits:
```bash
npx standard-version
```

## Integración de Git Hooks con Husky
Para utilizar husky:
1. Instalar husky: `npm install husky --save-dev`
2. Habilitar hooks: `npx husky install`
3. Añadir un hook pre-commit: `npx husky add .husky/pre-commit 'npm test'`

## Consejos y Mejores Prácticas
- Mantener los commits pequeños y enfocados en una sola tarea.
- Escribir mensajes claros y descriptivos.
- Usar la configuración de commitlint para mantener la coherencia.

## Comandos Git Útiles
- `git commit -m 'feat(ui): añadir botón de enviar'`
- `git log --oneline`
- `git checkout -b <nombre-de-la-rama>`

## Referencias
- [Conventional Commits](https://www.conventionalcommits.org)
- [Commitlint](https://commitlint.js.org)
- [Standard Version](https://github.com/conventional-changelog/standard-version)
- [Semantic Versioning](https://semver.org)
- [Git Hooks](https://git-scm.com/book/en/medr/Customizing-Git-Git-Hooks)
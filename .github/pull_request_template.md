# Pull Request - AgoraHUB360 ERP

## 📝 Descripción

<!-- Describe los cambios realizados en este PR -->

### Tipo de Cambio

<!-- Marca con 'x' el tipo de cambio que aplica -->

- [ ] 🐛 **Bug fix** (corrección de error)
- [ ] ✨ **New feature** (nueva funcionalidad)
- [ ] 💥 **Breaking change** (cambio que rompe compatibilidad)
- [ ] 📝 **Documentation** (actualización de documentación)
- [ ] ♻️ **Refactor** (refactorización de código)
- [ ] ⚡ **Performance** (mejora de rendimiento)
- [ ] ✅ **Tests** (agregar o modificar tests)
- [ ] 🔧 **Configuration** (cambios en configuración)
- [ ] 🗃️ **Database** (migraciones o cambios en BD)
- [ ] 🔒 **Security** (corrección de seguridad)

## 🎯 Issue Relacionado

<!-- Enlaza el issue que este PR resuelve -->
Closes #(issue)
<!-- O si está relacionado pero no lo cierra: -->
<!-- Related to #(issue) -->

## 💡 Motivación y Contexto

<!-- ¿Por qué se necesita este cambio? ¿Qué problema resuelve? -->

## 📋 Cambios Realizados

<!-- Lista detallada de los cambios -->

- 
- 
- 

## 🧪 Cómo se ha Probado

<!-- Describe las pruebas que realizaste -->

- [ ] Tests unitarios
- [ ] Tests de integración
- [ ] Tests end-to-end
- [ ] Pruebas manuales

### Escenarios de Prueba

<!-- Describe los escenarios específicos que probaste -->

1. 
2. 
3. 

## 📸 Screenshots / Videos

<!-- Si aplica, agrega capturas de pantalla o videos -->

### Antes

<!-- Captura del estado anterior -->

### Después

<!-- Captura del nuevo estado -->

## ✅ Checklist

### Código

- [ ] Mi código sigue los estándares de estilo del proyecto
- [ ] He realizado una auto-revisión de mi código
- [ ] He comentado mi código, especialmente en áreas complejas
- [ ] No hay warnings ni errores en la consola
- [ ] El código es DRY (Don't Repeat Yourself)
- [ ] He seguido los principios SOLID

### Testing

- [ ] He agregado tests que prueban mi funcionalidad
- [ ] Los tests existentes pasan localmente
- [ ] Los tests nuevos pasan localmente
- [ ] He verificado la cobertura de código
- [ ] He probado casos edge y escenarios de error

### Documentación

- [ ] He actualizado la documentación relevante
- [ ] He actualizado el README si es necesario
- [ ] He agregado comentarios de código donde es necesario
- [ ] He actualizado los docstrings/PHPDoc

### Base de Datos

- [ ] Las migraciones están incluidas
- [ ] Las migraciones son reversibles (down)
- [ ] Los seeders están actualizados si es necesario
- [ ] He verificado el impacto en datos existentes

### Seguridad

- [ ] No he expuesto credenciales o datos sensibles
- [ ] He validado todos los inputs del usuario
- [ ] He implementado autenticación/autorización si aplica
- [ ] He considerado vulnerabilidades comunes (SQL injection, XSS, CSRF)

### Performance

- [ ] He considerado el impacto en el rendimiento
- [ ] He optimizado consultas de base de datos
- [ ] He evitado N+1 queries
- [ ] He implementado caching si es necesario

### Git

- [ ] Mi rama está actualizada con la rama base
- [ ] No hay conflictos de merge
- [ ] Los commits siguen la convención (Conventional Commits)
- [ ] Los commits son atómicos y descriptivos

## 🔄 Migraciones de Base de Datos

<!-- Si este PR incluye migraciones, lista los cambios -->

### Nuevas Tablas

- 

### Tablas Modificadas

- 

### Comandos para Ejecutar

```bash
php artisan migrate
php artisan db:seed --class=NombreSeeder
```

## ⚠️ Breaking Changes

<!-- Si este PR introduce breaking changes, descríbelos en detalle -->

- [ ] Este PR introduce breaking changes

### Cambios Incompatibles

<!-- Lista los cambios que rompen compatibilidad -->

- 

### Guía de Migración

<!-- Instrucciones para migrar desde versión anterior -->

1. 
2. 
3. 

## 📦 Dependencias

<!-- Lista nuevas dependencias agregadas -->

### Composer
```json
"require": {
}
```

### NPM
```json
"dependencies": {
}
```

## 🔗 Enlaces Útiles

<!-- Enlaces a documentación, diseños, etc. -->

- Diseño en Figma: 
- Documentación técnica: 
- Issue en Jira: 

## 👥 Reviewers

<!-- Menciona a los revisores específicos si es necesario -->

@reviewer1 @reviewer2

## 📝 Notas Adicionales

<!-- Cualquier información adicional para los revisores -->

## 🎉 Checklist Post-Merge

<!-- Para ser completado después de mergear -->

- [ ] Actualizar documentación en wiki
- [ ] Comunicar cambios al equipo
- [ ] Actualizar CHANGELOG.md
- [ ] Crear issue para seguimiento si es necesario
- [ ] Eliminar rama feature

---

**Nota:** Por favor, asegúrate de completar todos los items del checklist antes de solicitar revisión.
# ?? RESETEO DE USUARIO ADMINISTRADOR - QUICK START

## ?? Uso Rápido

### Método 1: Automático (Recomendado)
```powershell
.\reset-admin-user.ps1
```
Escribe **`SI`** cuando pregunte y listo.

### Método 2: Generar SQL
```powershell
.\reset-admin-simple.ps1
```
Luego ejecuta el archivo `reset-admin-user.sql` en SSMS o Azure Data Studio.

---

## ?? Credenciales Resultantes

| Campo | Valor |
|-------|-------|
| **Email** | `admin@agorahub360.com` |
| **Usuario** | `admin` |
| **Contraseña** | `Admin123` |
| **Rol** | Admin |

---

## ? Verificación

1. Ejecutar reset
2. Iniciar sistema:
   ```powershell
   .\start-system.ps1
   ```
3. Ir a: `https://localhost:5002/login`
4. Login con credenciales de arriba

---

## ?? Importante

- ? Solo afecta usuario ID = 1
- ? Datos del sistema (productos, clientes, etc.) NO se eliminan
- ? Requiere confirmación
- ? En producción: cambiar contraseña inmediatamente

---

## ?? Solución Rápida de Problemas

### Error de conexión
```powershell
# Verificar SQL Server
Get-Service MSSQLSERVER

# Test de conexión
.\quick-db-test.ps1
```

### Login falla
```sql
-- Verificar hash de contraseña
SELECT PasswordHash FROM [core].[Usuarios] WHERE Id = 1;
-- Debe ser: jGQjcjUok1QVxKXOLRG+HuJ95WI0Mf/Xa6cOye1xICM=
```

---

## ?? Documentación Completa

Ver: [RESET-ADMIN-GUIDE.md](RESET-ADMIN-GUIDE.md)

---

**Fecha:** 2024-01-17  
**Scripts:** `reset-admin-user.ps1` | `reset-admin-simple.ps1`

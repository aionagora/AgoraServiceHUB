# ?? INSTRUCCIONES DE EJECUCIÓN - Empresa DEMO v2.0

## ?? Problema Detectado

El servidor SQL Server en `192.168.88.14,56885` no está accesible desde línea de comandos.

---

## ? SOLUCIÓN: Ejecutar Manualmente desde SQL Server Management Studio

### Opción 1: SQL Server Management Studio (RECOMENDADA)

1. **Abrir SSMS**
   - Inicie SQL Server Management Studio (SSMS)

2. **Conectarse al Servidor**
   ```
   Servidor: 192.168.88.14,56885
   Autenticación: SQL Server Authentication o Windows Authentication
   Usuario: sa (si usa SQL Auth)
   Password: [Su password]
   ```

3. **Abrir el Script**
   - Menú: `File` ? `Open` ? `File...`
   - Navegue a: `D:\AgoraCORE\AgoraHUB360-ERP\scripts\SeedData_EmpresaDEMO_v2.0.sql`
   - O simplemente arrastre el archivo a SSMS

4. **Verificar la Base de Datos**
   - En la barra de herramientas, asegúrese de que esté seleccionada: `db_AgoraERP_Core`
   - Si no aparece, puede cambiarla o el script lo hace automáticamente (tiene `USE [db_AgoraERP_Core]`)

5. **Ejecutar el Script**
   - Presione `F5` o haga clic en `Execute`
   - El script mostrará mensajes de progreso en la ventana de Messages

6. **Verificar Ejecución**
   - Si todo salió bien, verá:
     ```
     ? EMPRESA DEMO CREADA EXITOSAMENTE
     ```
   - Si hay error, la transacción se revierte automáticamente

---

### Opción 2: Azure Data Studio

1. **Abrir Azure Data Studio**

2. **Nueva Conexión**
   ```
   Connection type: Microsoft SQL Server
   Server: 192.168.88.14,56885
   Authentication: SQL Login o Windows Authentication
   User name: sa
   Password: [Su password]
   Database: db_AgoraERP_Core
   ```

3. **Abrir el Script**
   - `Ctrl+O` o `File` ? `Open File`
   - Seleccione: `SeedData_EmpresaDEMO_v2.0.sql`

4. **Ejecutar**
   - Presione `F5` o haga clic en `Run`

---

### Opción 3: Visual Studio

1. **Abrir Visual Studio**

2. **Server Explorer**
   - `View` ? `Server Explorer` (Ctrl+Alt+S)

3. **Agregar Conexión**
   - Clic derecho en `Data Connections` ? `Add Connection`
   - Servidor: `192.168.88.14,56885`
   - Base de datos: `db_AgoraERP_Core`

4. **Nueva Query**
   - Clic derecho en la conexión ? `New Query`
   - Copie y pegue todo el contenido de `SeedData_EmpresaDEMO_v2.0.sql`

5. **Ejecutar**
   - Presione `Ctrl+Shift+E`

---

## ?? VERIFICACIÓN POST-EJECUCIÓN

Después de ejecutar el script exitosamente, verifique que se creó todo:

```sql
-- 1. Verificar empresa
SELECT * FROM [core].[Empresas] 
WHERE NIT = '900123456-1';

-- 2. Verificar usuarios
SELECT u.NombreUsuario, u.Email, ue.Rol
FROM [core].[Usuarios] u
INNER JOIN [core].[UsuarioEmpresas] ue ON u.Id = ue.UsuarioId
WHERE u.NombreUsuario IN ('admin.muebles', 'vendedor.juan', 'bodega.maria');

-- 3. Verificar productos
SELECT COUNT(*) AS TotalProductos
FROM [mdm].[Productos]
WHERE EmpresaId = (SELECT Id FROM [core].[Empresas] WHERE NIT = '900123456-1');
-- Debe devolver: 15

-- 4. Verificar stock
SELECT COUNT(*) AS TotalStock
FROM [inv].[StockProductos]
WHERE EmpresaId = (SELECT Id FROM [core].[Empresas] WHERE NIT = '900123456-1');
-- Debe devolver: 15

-- 5. Verificar clientes
SELECT COUNT(*) AS TotalClientes
FROM [mdm].[Clientes]
WHERE EmpresaId = (SELECT Id FROM [core].[Empresas] WHERE NIT = '900123456-1');
-- Debe devolver: 5
```

---

## ?? CREDENCIALES DE ACCESO

Una vez creada la empresa DEMO, use estas credenciales en la aplicación web:

| Usuario | Password | Rol |
|---------|----------|-----|
| `admin.muebles` | `Admin123` | Administrador |
| `vendedor.juan` | `Admin123` | Vendedor |
| `bodega.maria` | `Admin123` | Bodeguero |

---

## ?? SOLUCIÓN DE PROBLEMAS

### Error: "Ya existe una empresa con NIT..."
Si el script ya se ejecutó antes, debe eliminar los datos primero:

```sql
-- ADVERTENCIA: Esto eliminará TODOS los datos de la empresa DEMO
DECLARE @EmpresaId INT = (SELECT Id FROM [core].[Empresas] WHERE NIT = '900123456-1');

BEGIN TRANSACTION;

DELETE FROM [inv].[StockProductos] WHERE EmpresaId = @EmpresaId;
DELETE FROM [mdm].[Productos] WHERE EmpresaId = @EmpresaId;
DELETE FROM [mdm].[CategoriasProducto] WHERE EmpresaId = @EmpresaId;
DELETE FROM [mdm].[UnidadMedida] WHERE EmpresaId = @EmpresaId;
DELETE FROM [mdm].[Almacenes] WHERE EmpresaId = @EmpresaId;
DELETE FROM [mdm].[Clientes] WHERE EmpresaId = @EmpresaId;
DELETE FROM [mdm].[Proveedores] WHERE EmpresaId = @EmpresaId;
DELETE FROM [core].[NumeracionDocumentos] WHERE EmpresaId = @EmpresaId;
DELETE FROM [core].[UsuarioEmpresas] WHERE EmpresaId = @EmpresaId;
DELETE FROM [core].[Usuarios] WHERE EmpresaActivaId = @EmpresaId;
DELETE FROM [core].[Empresas] WHERE Id = @EmpresaId;

COMMIT TRANSACTION;

-- Luego ejecute nuevamente el script SeedData_EmpresaDEMO_v2.0.sql
```

### Error: "Cannot insert duplicate key..."
Esto significa que ya existen algunos datos. Use el script de limpieza anterior.

### Error de conexión
Verifique que:
- El servidor SQL Server esté ejecutándose
- El puerto 56885 esté abierto
- Tenga los permisos necesarios
- La IP 192.168.88.14 sea accesible desde su máquina

---

## ?? SOPORTE

Si tiene problemas para ejecutar el script:

1. Verifique que la base de datos `db_AgoraERP_Core` exista
2. Asegúrese de tener permisos de escritura en todas las tablas
3. Revise que las migraciones de Entity Framework estén aplicadas
4. Consulte los logs de SQL Server para errores específicos

---

**Archivo del Script**: `D:\AgoraCORE\AgoraHUB360-ERP\scripts\SeedData_EmpresaDEMO_v2.0.sql`

**Versión**: 2.0  
**Fecha**: 2026-02-19

# Script para verificar que el usuario administrador fue creado correctamente
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? Verificación de Usuario Administrador" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Leer la cadena de conexión del appsettings.json
$appsettingsPath = "src\AgoraHub360.ERP.Api\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    
    # Extraer componentes de la cadena de conexión
    if ($connectionString -match "Server=([^;]+)") { $server = $matches[1] }
    if ($connectionString -match "Database=([^;]+)") { $database = $matches[1] }
    if ($connectionString -match "User Id=([^;]+)") { $userId = $matches[1] }
    if ($connectionString -match "Password=([^;]+)") { $password = $matches[1] }
    
    Write-Host "?? Conexión a Base de Datos:" -ForegroundColor Yellow
    Write-Host "   Servidor: $server" -ForegroundColor White
    Write-Host "   Base de datos: $database" -ForegroundColor White
    Write-Host "   Usuario: $userId" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "? No se encontró appsettings.json" -ForegroundColor Red
    exit 1
}

# Verificar que sqlcmd esté disponible
if (!(Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    Write-Host "?? sqlcmd no está instalado. Instala SQL Server Command Line Tools." -ForegroundColor Yellow
    Write-Host "   Descarga: https://aka.ms/sqlcmd" -ForegroundColor Cyan
    exit 1
}

Write-Host "?? Verificando usuario administrador..." -ForegroundColor Yellow
Write-Host ""

# Query para verificar el usuario
$query = @"
-- Verificar usuario admin
SELECT 
    'Usuario' AS Tipo,
    Id,
    NombreUsuario,
    Email,
    NombreCompleto,
    CASE WHEN Activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado,
    CONVERT(varchar, FechaCreacion, 120) AS FechaCreacion
FROM [core].[Usuarios]
WHERE NombreUsuario = 'admin';

-- Verificar empresa demo
SELECT 
    'Empresa' AS Tipo,
    Id,
    Nombre,
    NIT,
    Email,
    MonedaBaseId,
    CASE WHEN Activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado
FROM [core].[Empresas]
WHERE Id = 1;

-- Verificar asignación de rol
SELECT 
    'Asignacion' AS Tipo,
    u.NombreUsuario AS Usuario,
    e.Nombre AS Empresa,
    ue.Rol
FROM [core].[UsuarioEmpresas] ue
INNER JOIN [core].[Usuarios] u ON ue.UsuarioId = u.Id
INNER JOIN [core].[Empresas] e ON ue.EmpresaId = e.Id
WHERE u.NombreUsuario = 'admin' AND e.Id = 1;
"@

try {
    $result = sqlcmd -S $server -U $userId -P $password -d $database -Q $query -s "|" -W 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Consulta ejecutada correctamente" -ForegroundColor Green
        Write-Host ""
        Write-Host "?? Resultados:" -ForegroundColor Cyan
        Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor DarkGray
        Write-Host $result -ForegroundColor White
        Write-Host ""
        
        # Verificar si se encontraron resultados
        if ($result -match "admin") {
            Write-Host "? Usuario administrador encontrado" -ForegroundColor Green
            Write-Host ""
            Write-Host "?? Credenciales de Acceso:" -ForegroundColor Yellow
            Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor DarkGray
            Write-Host "   Email:      " -NoNewline -ForegroundColor White
            Write-Host "admin@agorahub360.com" -ForegroundColor Cyan
            Write-Host "   Contraseña: " -NoNewline -ForegroundColor White
            Write-Host "Admin123" -ForegroundColor Cyan
            Write-Host "   Rol:        " -NoNewline -ForegroundColor White
            Write-Host "Admin" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "?? URLs de Acceso:" -ForegroundColor Yellow
            Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor DarkGray
            Write-Host "   Aplicación Web: " -NoNewline -ForegroundColor White
            Write-Host "https://localhost:5002" -ForegroundColor Cyan
            Write-Host "   API:            " -NoNewline -ForegroundColor White
            Write-Host "https://localhost:7001" -ForegroundColor Cyan
            Write-Host "   Swagger:        " -NoNewline -ForegroundColor White
            Write-Host "https://localhost:7001/swagger" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "?? Tip:" -ForegroundColor Yellow
            Write-Host "   Para más información, consulta: " -NoNewline -ForegroundColor White
            Write-Host "CREDENCIALES-DEFAULT.md" -ForegroundColor Cyan
            Write-Host ""
        } else {
            Write-Host "?? Usuario administrador NO encontrado" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "?? Solución:" -ForegroundColor Yellow
            Write-Host "   Ejecuta las migraciones para crear el usuario:" -ForegroundColor White
            Write-Host "   " -NoNewline
            Write-Host "dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api" -ForegroundColor Cyan
            Write-Host ""
        }
    } else {
        Write-Host "? Error al ejecutar la consulta: $result" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error de conexión: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Verifica:" -ForegroundColor Yellow
    Write-Host "   1. Que SQL Server esté corriendo" -ForegroundColor White
    Write-Host "   2. Que la cadena de conexión sea correcta" -ForegroundColor White
    Write-Host "   3. Que las credenciales tengan permisos" -ForegroundColor White
}

Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Script rápido para testear la base de datos sin iniciar la API manualmente
# Este script usa EF Core directamente para probar la conexión

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AgoraHub360 ERP - Quick DB Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Cambiar al directorio del proyecto de Persistence
Set-Location "src\AgoraHub360.ERP.Persistence"

Write-Host "?? Probando conexión a la base de datos..." -ForegroundColor Yellow
Write-Host ""

# Test 1: Verificar que la base de datos existe y puede conectarse
Write-Host "1?? Test: Verificando conectividad..." -ForegroundColor Green
try {
    $efTestOutput = dotnet ef database drop --dry-run --project . --startup-project ..\AgoraHub360.ERP.Api 2>&1
    
    if ($LASTEXITCODE -eq 0 -or $efTestOutput -match "database") {
        Write-Host "   ? Entity Framework puede acceder a la configuración" -ForegroundColor Green
    } else {
        Write-Host "   ?? Advertencia: $efTestOutput" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ? Error: $_" -ForegroundColor Red
}

# Test 2: Listar migraciones aplicadas
Write-Host "`n2?? Test: Verificando migraciones..." -ForegroundColor Green
try {
    Write-Host "   Consultando estado de migraciones..." -ForegroundColor Gray
    $migrationsOutput = dotnet ef migrations list --project . --startup-project ..\AgoraHub360.ERP.Api 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Comando ejecutado correctamente" -ForegroundColor Green
        Write-Host ""
        Write-Host "   Migraciones:" -ForegroundColor Cyan
        Write-Host "   $migrationsOutput" -ForegroundColor White
    } else {
        Write-Host "   ? Error al listar migraciones: $migrationsOutput" -ForegroundColor Red
    }
} catch {
    Write-Host "   ? Error: $_" -ForegroundColor Red
}

# Test 3: Intentar conectar a la base de datos usando dotnet ef
Write-Host "`n3?? Test: Probando conexión directa a SQL Server..." -ForegroundColor Green

# Leer la cadena de conexión del appsettings.json
$appsettingsPath = "..\AgoraHub360.ERP.Api\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    
    # Extraer componentes de la cadena de conexión
    if ($connectionString -match "Server=([^;]+)") { $server = $matches[1] }
    if ($connectionString -match "Database=([^;]+)") { $database = $matches[1] }
    if ($connectionString -match "User Id=([^;]+)") { $userId = $matches[1] }
    
    Write-Host "   Servidor: $server" -ForegroundColor Gray
    Write-Host "   Base de datos: $database" -ForegroundColor Gray
    Write-Host "   Usuario: $userId" -ForegroundColor Gray
    Write-Host ""
    
    # Intentar ping al servidor (extraer IP/hostname)
    if ($server -match "^([^,\\]+)") {
        $serverHost = $matches[1]
        Write-Host "   Probando conectividad de red a $serverHost..." -ForegroundColor Gray
        
        try {
            $pingResult = Test-Connection -ComputerName $serverHost -Count 1 -ErrorAction Stop
            Write-Host "   ? Servidor responde a ping" -ForegroundColor Green
            Write-Host "   IP: $($pingResult.IPV4Address)" -ForegroundColor Gray
        } catch {
            Write-Host "   ?? No se puede hacer ping al servidor (puede estar bloqueado por firewall)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "   ?? No se encontró appsettings.json" -ForegroundColor Yellow
}

# Test 4: Ejecutar un script SQL de prueba si sqlcmd está disponible
Write-Host "`n4?? Test: Intentando consulta SQL directa..." -ForegroundColor Green

if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
    Write-Host "   sqlcmd está disponible, ejecutando consulta..." -ForegroundColor Gray
    
    # Extraer password de manera segura (para propósitos de testing)
    if ($connectionString -match "Password=([^;]+)") { 
        $password = $matches[1] 
        
        # Ejecutar consulta simple
        try {
            $sqlResult = sqlcmd -S $server -U $userId -P $password -d $database -Q "SELECT @@VERSION as SQLVersion, DB_NAME() as DatabaseName" -h -1 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ? Conexión SQL exitosa!" -ForegroundColor Green
                Write-Host ""
                Write-Host "   Resultado:" -ForegroundColor Cyan
                Write-Host "   $sqlResult" -ForegroundColor White
            } else {
                Write-Host "   ? Error de conexión SQL: $sqlResult" -ForegroundColor Red
            }
        } catch {
            Write-Host "   ? Error: $_" -ForegroundColor Red
        }
    }
} else {
    Write-Host "   ?? sqlcmd no está instalado. Instala SQL Server Command Line Tools para más tests." -ForegroundColor Yellow
    Write-Host "   Descarga: https://aka.ms/sqlcmd" -ForegroundColor Cyan
}

# Volver al directorio raíz
Set-Location ..\..

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Tests Completados" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Para un test más completo, inicia la API y ejecuta:" -ForegroundColor Yellow
Write-Host "   .\test-database-connection.ps1" -ForegroundColor Cyan
Write-Host ""

# DIAGNOSTICO DE CONEXIONES - AgoraHub360 ERP

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host " DIAGNOSTICO DE CONEXIONES" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# 1. VERIFICAR PUERTOS DE LA API
Write-Host "1. Verificando si la API esta corriendo en puerto 7001..." -ForegroundColor Yellow
$puerto7001 = Get-NetTCPConnection -LocalPort 7001 -ErrorAction SilentlyContinue

if ($puerto7001) {
    Write-Host "   OK - API CORRIENDO en puerto 7001" -ForegroundColor Green
    Write-Host "   Estado: $($puerto7001.State)" -ForegroundColor Green
} else {
    Write-Host "   ERROR - API NO esta corriendo en puerto 7001" -ForegroundColor Red
    Write-Host "   Solucion: Ejecuta la API con:" -ForegroundColor Yellow
    Write-Host "      cd src\AgoraHub360.ERP.Api" -ForegroundColor Gray
    Write-Host "      dotnet run" -ForegroundColor Gray
}
Write-Host ""

# 2. VERIFICAR PUERTOS DE BLAZOR WASM
Write-Host "2. Verificando si Blazor WASM esta corriendo en puerto 5002..." -ForegroundColor Yellow
$puerto5002 = Get-NetTCPConnection -LocalPort 5002 -ErrorAction SilentlyContinue

if ($puerto5002) {
    Write-Host "   OK - BLAZOR WASM CORRIENDO en puerto 5002" -ForegroundColor Green
    Write-Host "   Estado: $($puerto5002.State)" -ForegroundColor Green
} else {
    Write-Host "   ERROR - Blazor WASM NO esta corriendo en puerto 5002" -ForegroundColor Red
    Write-Host "   Solucion: Ejecuta Blazor con:" -ForegroundColor Yellow
    Write-Host "      cd src\AgoraHub360.ERP.Web" -ForegroundColor Gray
    Write-Host "      dotnet run" -ForegroundColor Gray
}
Write-Host ""

# 3. VERIFICAR CONEXION A LA BASE DE DATOS
Write-Host "3. Verificando conexion a SQL Server..." -ForegroundColor Yellow
$servidor = "192.168.88.14"
$puerto = 56885

$conexionSQL = Test-NetConnection -ComputerName $servidor -Port $puerto -WarningAction SilentlyContinue

if ($conexionSQL.TcpTestSucceeded) {
    Write-Host "   OK - CONEXION EXITOSA a SQL Server" -ForegroundColor Green
    Write-Host "   Servidor: $servidor`:$puerto" -ForegroundColor Green
} else {
    Write-Host "   ERROR - NO SE PUEDE CONECTAR a SQL Server" -ForegroundColor Red
    Write-Host "   Servidor: $servidor`:$puerto" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Posibles soluciones:" -ForegroundColor Yellow
    Write-Host "      - Verifica que SQL Server este corriendo" -ForegroundColor Gray
    Write-Host "      - Verifica que el firewall permita el puerto $puerto" -ForegroundColor Gray
    Write-Host "      - Verifica que la IP $servidor sea correcta" -ForegroundColor Gray
    Write-Host "      - Verifica que SQL Server acepte conexiones remotas" -ForegroundColor Gray
}
Write-Host ""

# 4. VERIFICAR CONECTIVIDAD API DESDE BLAZOR
Write-Host "4. Verificando conectividad a la API desde Blazor..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "https://localhost:7001/health" -UseBasicParsing -SkipCertificateCheck -TimeoutSec 5 -ErrorAction Stop
    Write-Host "   OK - API RESPONDE correctamente" -ForegroundColor Green
    Write-Host "   Health Check: OK" -ForegroundColor Green
} catch {
    Write-Host "   ERROR - API NO RESPONDE" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Posibles soluciones:" -ForegroundColor Yellow
    Write-Host "      - Asegurate de que la API este corriendo (dotnet run)" -ForegroundColor Gray
    Write-Host "      - Verifica el certificado SSL de desarrollo" -ForegroundColor Gray
    Write-Host "      - Revisa los logs de la API en la consola" -ForegroundColor Gray
}
Write-Host ""

# 5. RESUMEN DE CONFIGURACION
Write-Host "5. Configuracion actual:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   API:" -ForegroundColor Cyan
Write-Host "      - URL: https://localhost:7001/" -ForegroundColor Gray
Write-Host "      - Puerto: 7001" -ForegroundColor Gray
Write-Host "      - CORS permite: https://localhost:5002" -ForegroundColor Gray
Write-Host ""
Write-Host "   Blazor WebAssembly:" -ForegroundColor Cyan
Write-Host "      - URL: https://localhost:5002/" -ForegroundColor Gray
Write-Host "      - Puerto: 5002" -ForegroundColor Gray
Write-Host "      - Apunta a API: https://localhost:7001/" -ForegroundColor Gray
Write-Host ""
Write-Host "   Base de Datos:" -ForegroundColor Cyan
Write-Host "      - Servidor: 192.168.88.14,56885" -ForegroundColor Gray
Write-Host "      - Base de Datos: db_AgoraERP_Core" -ForegroundColor Gray
Write-Host "      - Usuario: usagora" -ForegroundColor Gray
Write-Host ""

# 6. INSTRUCCIONES FINALES
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host " PASOS PARA INICIAR EL SISTEMA" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Iniciar la API:" -ForegroundColor Yellow
Write-Host "   cd src\AgoraHub360.ERP.Api" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Iniciar Blazor (en otra terminal):" -ForegroundColor Yellow
Write-Host "   cd src\AgoraHub360.ERP.Web" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Abrir el navegador:" -ForegroundColor Yellow
Write-Host "   https://localhost:5002" -ForegroundColor Gray
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

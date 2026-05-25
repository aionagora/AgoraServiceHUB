@echo off
echo ==========================================
echo  VERIFICACION DE PUERTOS Y CONFIGURACION
echo ==========================================
echo.

echo Configuracion actual:
echo.
echo API (launchSettings.json):
echo   - Profile HTTPS: https://localhost:7001
echo   - Profile HTTP:  http://localhost:5000
echo   - IIS Express:   http://localhost:38161 (SSL: 44319)
echo.
echo Blazor WebAssembly (launchSettings.json):
echo   - Profile HTTPS: https://localhost:5002
echo   - Profile HTTP:  http://localhost:5001
echo   - IIS Express:   http://localhost:18900 (SSL: 44394)
echo.
echo Configuracion en appsettings.json:
echo   - Blazor apunta a API: https://localhost:7001/
echo   - API permite CORS de: https://localhost:5002
echo.

echo Verificando si los puertos estan en uso...
echo.

powershell -Command "$ports = @(7001, 5000, 5002, 5001, 38161, 44319, 18900, 44394); foreach ($port in $ports) { $conn = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue; if ($conn) { Write-Host \"Puerto $port`: EN USO (PID: $($conn.OwningProcess))\" -ForegroundColor Yellow } else { Write-Host \"Puerto $port`: LIBRE\" -ForegroundColor Green } }"

echo.
echo ==========================================
echo  RECOMENDACION
echo ==========================================
echo.
echo Si los puertos 7001 y 5002 estan LIBRES:
echo   1. Usa los archivos .bat para iniciar
echo   2. API correra en https://localhost:7001
echo   3. Blazor correra en https://localhost:5002
echo.
echo Si algun puerto esta EN USO:
echo   Opcion A: Detener el proceso que lo usa
echo   Opcion B: Cambiar los puertos en launchSettings.json
echo.
pause

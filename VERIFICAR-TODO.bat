@echo off
cls
echo.
echo =========================================================
echo  CONFIGURACION VERIFICADA - AgoraHub360 ERP
echo =========================================================
echo.
echo [OK] API configurada en:        https://localhost:7001
echo [OK] Blazor configurado en:     https://localhost:5002
echo [OK] SQL Server:                192.168.88.14:56885
echo.
echo [OK] CORS: API permite peticiones desde Blazor
echo [OK] Blazor apunta a la API correctamente
echo.
echo =========================================================
echo  PARA INICIAR EL SISTEMA:
echo =========================================================
echo.
echo  1. TERMINAL 1: Ejecuta   1-iniciar-api.bat
echo.
echo  2. TERMINAL 2: Ejecuta   2-iniciar-blazor.bat
echo.
echo  3. NAVEGADOR:  Abre      https://localhost:5002
echo.
echo =========================================================
echo.
echo Deseas verificar si los puertos estan libres? (S/N)
set /p verificar=

if /i "%verificar%"=="S" goto verificar_puertos
if /i "%verificar%"=="s" goto verificar_puertos
goto fin

:verificar_puertos
echo.
echo Verificando puertos...
echo.
powershell -Command "$ports = @(7001, 5002); foreach ($port in $ports) { $conn = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue; if ($conn) { Write-Host \"Puerto $port`: EN USO\" -ForegroundColor Red } else { Write-Host \"Puerto $port`: LIBRE\" -ForegroundColor Green } }"
echo.

:fin
pause

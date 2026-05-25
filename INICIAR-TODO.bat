@echo off
REM =========================================================
REM  INICIO AUTOMATICO - AgoraHub360 ERP
REM  Este script inicia API y Blazor en ventanas separadas
REM =========================================================

echo.
echo =========================================================
echo  INICIO AUTOMATICO - AgoraHub360 ERP
echo =========================================================
echo.
echo Este script iniciara:
echo   1. API en https://localhost:7001 (ventana separada)
echo   2. Blazor en https://localhost:5002 (ventana separada)
echo.
echo IMPORTANTE: Ambas ventanas deben permanecer abiertas
echo.
pause

echo.
echo Iniciando API...
start "API - AgoraHub360 ERP" cmd /k "cd /d %~dp0 && 1-iniciar-api.bat"

echo Esperando 5 segundos para que la API inicie...
timeout /t 5 /nobreak >nul

echo.
echo Iniciando Blazor...
start "Blazor - AgoraHub360 ERP" cmd /k "cd /d %~dp0 && 2-iniciar-blazor.bat"

echo.
echo =========================================================
echo  SISTEMA INICIADO
echo =========================================================
echo.
echo Se han abierto 2 ventanas:
echo   - API:    https://localhost:7001
echo   - Blazor: https://localhost:5002
echo.
echo Esperando 10 segundos antes de abrir el navegador...
timeout /t 10 /nobreak >nul

echo.
echo Abriendo navegador...
start https://localhost:5002

echo.
echo =========================================================
echo  INSTRUCCIONES:
echo =========================================================
echo.
echo - Se abrio el navegador en https://localhost:5002
echo - Si no ves la pagina de login, espera unos segundos mas
echo - Para DETENER el sistema, cierra las 2 ventanas abiertas
echo - NO cierres esta ventana hasta terminar de usar el sistema
echo.
echo Presiona cualquier tecla para cerrar esta ventana
echo (El sistema seguira corriendo en las otras ventanas)
echo.
pause

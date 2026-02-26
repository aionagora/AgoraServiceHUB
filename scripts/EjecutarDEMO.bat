@echo off
echo ================================================================
echo EJECUTANDO SCRIPT DE EMPRESA DEMO - MUEBLES DEL MUNDO S.A.
echo ================================================================
echo.
echo Conectando al servidor: 192.168.88.14,56885
echo Base de datos: db_AgoraERP_Core
echo.

REM Intentar con autenticación SQL
echo Intento 1: Autenticacion SQL con usuario sa...
sqlcmd -S 192.168.88.14,56885 -U sa -P "QazWsx10!.2025" -d db_AgoraERP_Core -i "%~dp0SeedData_EmpresaDEMO_v2.0.sql"

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo Intento 2: Autenticacion de Windows...
    sqlcmd -S 192.168.88.14,56885 -E -d db_AgoraERP_Core -i "%~dp0SeedData_EmpresaDEMO_v2.0.sql"
)

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo ================================================================
    echo ERROR: No se pudo conectar al servidor
    echo ================================================================
    echo.
    echo Por favor, ejecute el script manualmente:
    echo 1. Abra SQL Server Management Studio
    echo 2. Conectese al servidor: 192.168.88.14,56885
    echo 3. Abra el archivo: %~dp0SeedData_EmpresaDEMO_v2.0.sql
    echo 4. Ejecute el script (F5)
    echo.
    pause
    exit /b 1
)

echo.
echo ================================================================
echo SCRIPT EJECUTADO EXITOSAMENTE
echo ================================================================
echo.
pause

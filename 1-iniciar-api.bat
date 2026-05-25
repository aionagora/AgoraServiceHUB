@echo off
echo ==========================================
echo  INICIANDO API - AgoraHub360 ERP
echo ==========================================
echo.
echo Puerto configurado: https://localhost:7001
echo Profile: https (launchSettings.json)
echo.

cd src\AgoraHub360.ERP.Api

echo Compilando API...
dotnet build --configuration Debug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: La API no compilo correctamente
    echo Revisa los errores arriba
    pause
    exit /b 1
)

echo.
echo ==========================================
echo API compilada correctamente
echo.
echo IMPORTANTE:
echo   - La API se iniciara en: https://localhost:7001
echo   - Swagger disponible en: https://localhost:7001/swagger
echo   - Health check en: https://localhost:7001/health
echo.
echo Presiona Ctrl+C para detener
echo ==========================================
echo.

dotnet run --launch-profile https

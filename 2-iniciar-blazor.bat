@echo off
echo ==========================================
echo  INICIANDO BLAZOR WASM - AgoraHub360 ERP
echo ==========================================
echo.
echo Puerto configurado: https://localhost:5002
echo API target: https://localhost:7001
echo Profile: https (launchSettings.json)
echo.

cd src\AgoraHub360.ERP.Web

echo Compilando Blazor WebAssembly...
dotnet build --configuration Debug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Blazor no compilo correctamente
    echo Revisa los errores arriba
    pause
    exit /b 1
)

echo.
echo ==========================================
echo Blazor compilado correctamente
echo.
echo IMPORTANTE:
echo   - Blazor se iniciara en: https://localhost:5002
echo   - Debe apuntar a API en: https://localhost:7001
echo   - Asegurate de que la API este corriendo PRIMERO
echo.
echo Presiona Ctrl+C para detener
echo ==========================================
echo.

dotnet run --launch-profile https

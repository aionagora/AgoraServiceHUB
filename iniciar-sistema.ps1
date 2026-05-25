# SCRIPT DE INICIO - AgoraHub360 ERP
# Este script inicia la API y el cliente Blazor

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host " INICIANDO AGORAHUB360 ERP" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que los proyectos existan
$apiPath = "src\AgoraHub360.ERP.Api\AgoraHub360.ERP.Api.csproj"
$webPath = "src\AgoraHub360.ERP.Web\AgoraHub360.ERP.Web.csproj"

if (-not (Test-Path $apiPath)) {
    Write-Host "ERROR: No se encuentra el proyecto API en $apiPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $webPath)) {
    Write-Host "ERROR: No se encuentra el proyecto Web en $webPath" -ForegroundColor Red
    exit 1
}

Write-Host "OK - Proyectos encontrados" -ForegroundColor Green
Write-Host ""

# Mostrar configuracion
Write-Host "Configuracion:" -ForegroundColor Yellow
Write-Host "   API: https://localhost:7001/" -ForegroundColor Gray
Write-Host "   WEB: https://localhost:5002/" -ForegroundColor Gray
Write-Host "   BD:  192.168.88.14,56885" -ForegroundColor Gray
Write-Host ""

# Instrucciones
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host " PARA INICIAR EL SISTEMA:" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "TERMINAL 1 - API:" -ForegroundColor Yellow
Write-Host "   cd src\AgoraHub360.ERP.Api" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "TERMINAL 2 - Blazor WebAssembly:" -ForegroundColor Yellow
Write-Host "   cd src\AgoraHub360.ERP.Web" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "NAVEGADOR:" -ForegroundColor Yellow
Write-Host "   https://localhost:5002" -ForegroundColor White
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

$respuesta = Read-Host "Deseas iniciar la API ahora? (S/N)"

if ($respuesta -eq "S" -or $respuesta -eq "s") {
    Write-Host ""
    Write-Host "Iniciando API..." -ForegroundColor Green
    Write-Host "Presiona Ctrl+C para detener" -ForegroundColor Gray
    Write-Host ""
    
    Set-Location src\AgoraHub360.ERP.Api
    dotnet run
}

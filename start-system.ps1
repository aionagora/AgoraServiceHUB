# Script para iniciar el sistema AgoraHub360 ERP completo
Write-Host "`n???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? Iniciando AgoraHub360 ERP" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Función para detener procesos existentes
function Stop-ExistingProcesses {
    Write-Host "?? Verificando procesos existentes..." -ForegroundColor Yellow
    
    $apiProcesses = Get-Process | Where-Object {$_.ProcessName -eq "AgoraHub360.ERP.Api"}
    if ($apiProcesses) {
        Write-Host "   Deteniendo API existente..." -ForegroundColor Yellow
        $apiProcesses | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
        Start-Sleep -Seconds 2
        Write-Host "   ? API detenida" -ForegroundColor Green
    }
    
    # Buscar procesos de dotnet que puedan ser la Web
    $webProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
        Where-Object { $_.CommandLine -like "*AgoraHub360.ERP.Web*" }
    if ($webProcesses) {
        Write-Host "   Deteniendo Web existente..." -ForegroundColor Yellow
        $webProcesses | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
        Start-Sleep -Seconds 2
        Write-Host "   ? Web detenida" -ForegroundColor Green
    }
    
    Write-Host ""
}

# Función para iniciar la API
function Start-API {
    Write-Host "1?? Iniciando API..." -ForegroundColor Yellow
    Write-Host "   Puerto HTTPS: 7001" -ForegroundColor Gray
    Write-Host "   Puerto HTTP:  5000" -ForegroundColor Gray
    Write-Host ""
    
    $apiPath = "src\AgoraHub360.ERP.Api"
    
    # Iniciar la API en una nueva ventana de PowerShell
    $apiScript = @"
Set-Location '$pwd\$apiPath'
Write-Host '???????????????????????????????????????????????????????????????' -ForegroundColor Cyan
Write-Host '  API - AgoraHub360 ERP' -ForegroundColor Green
Write-Host '???????????????????????????????????????????????????????????????' -ForegroundColor Cyan
Write-Host ''
dotnet run --launch-profile https
"@
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $apiScript
    
    Write-Host "   ? Esperando a que la API inicie..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    # Verificar que la API esté respondiendo
    $maxAttempts = 10
    $attempt = 0
    $apiReady = $false
    
    while (-not $apiReady -and $attempt -lt $maxAttempts) {
        try {
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
            $response = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/diagnostics/ping" -ErrorAction Stop
            if ($response.status -eq "ok") {
                $apiReady = $true
                Write-Host "   ? API iniciada correctamente" -ForegroundColor Green
                Write-Host "   ?? Swagger: https://localhost:7001/swagger" -ForegroundColor Cyan
            }
        } catch {
            $attempt++
            if ($attempt -lt $maxAttempts) {
                Write-Host "   ? Intento $attempt/$maxAttempts..." -ForegroundColor Gray
                Start-Sleep -Seconds 2
            }
        }
    }
    
    if (-not $apiReady) {
        Write-Host "   ?? La API puede no estar completamente lista. Revisa la ventana de la API." -ForegroundColor Yellow
    }
    
    Write-Host ""
}

# Función para iniciar la aplicación Web
function Start-Web {
    Write-Host "2?? Iniciando Aplicación Web..." -ForegroundColor Yellow
    Write-Host "   Puerto HTTPS: 5002" -ForegroundColor Gray
    Write-Host "   Puerto HTTP:  5001" -ForegroundColor Gray
    Write-Host ""
    
    $webPath = "src\AgoraHub360.ERP.Web"
    
    # Iniciar la Web en una nueva ventana de PowerShell
    $webScript = @"
Set-Location '$pwd\$webPath'
Write-Host '???????????????????????????????????????????????????????????????' -ForegroundColor Cyan
Write-Host '  WEB - AgoraHub360 ERP' -ForegroundColor Green
Write-Host '???????????????????????????????????????????????????????????????' -ForegroundColor Cyan
Write-Host ''
dotnet run --launch-profile https
"@
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $webScript
    
    Write-Host "   ? Esperando a que la Web inicie..." -ForegroundColor Yellow
    Start-Sleep -Seconds 8
    
    Write-Host "   ? Web iniciada" -ForegroundColor Green
    Write-Host "   ?? Login: https://localhost:5002/login" -ForegroundColor Cyan
    Write-Host ""
}

# Ejecución principal
try {
    Stop-ExistingProcesses
    Start-API
    Start-Web
    
    Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "  ? Sistema Iniciado Correctamente" -ForegroundColor Green
    Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "?? Credenciales de Acceso:" -ForegroundColor Yellow
    Write-Host "   Email:      admin@agorahub360.com" -ForegroundColor Cyan
    Write-Host "   Contraseña: Admin123" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "?? URLs Disponibles:" -ForegroundColor Yellow
    Write-Host "   Login:   https://localhost:5002/login" -ForegroundColor Cyan
    Write-Host "   Web:     https://localhost:5002" -ForegroundColor Cyan
    Write-Host "   Swagger: https://localhost:7001/swagger" -ForegroundColor Cyan
    Write-Host "   Health:  https://localhost:7001/health" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "?? Tip:" -ForegroundColor Yellow
    Write-Host "   - Las aplicaciones están corriendo en ventanas separadas" -ForegroundColor White
    Write-Host "   - Para detenerlas, cierra las ventanas o presiona Ctrl+C en cada una" -ForegroundColor White
    Write-Host "   - Los logs se mostrarán en las respectivas ventanas" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Documentación: CREDENCIALES-DEFAULT.md" -ForegroundColor Gray
    Write-Host ""
    Write-Host "¡Abriendo el navegador en el login...!" -ForegroundColor Green
    Start-Sleep -Seconds 2
    Start-Process "https://localhost:5002/login"
    
} catch {
    Write-Host "? Error al iniciar el sistema: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "Presiona cualquier tecla para salir..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

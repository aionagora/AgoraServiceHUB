# Script de limpieza y reinicio de servicios AgoraHub360 ERP
# Ejecutar desde la raíz del repositorio

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   REINICIO COMPLETO - AgoraHub360 ERP" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Detener servicios corriendo
Write-Host "[1/8] Deteniendo servicios existentes..." -ForegroundColor Yellow

$apiProcesses = Get-Process -Name "AgoraHub360.ERP.Api" -ErrorAction SilentlyContinue
if ($apiProcesses) {
    $apiProcesses | Stop-Process -Force
    Write-Host "  ✓ API detenida" -ForegroundColor Green
}

$webProcesses = Get-Process -Name "AgoraHub360.ERP.Web" -ErrorAction SilentlyContinue
if ($webProcesses) {
    $webProcesses | Stop-Process -Force
    Write-Host "  ✓ Web detenida" -ForegroundColor Green
}

# Paso 2: Matar procesos dotnet ocupando puertos 7001 y 5002
Write-Host "[2/8] Liberando puertos 7001 y 5002..." -ForegroundColor Yellow

$netstat = netstat -ano | Select-String ":7001" -Context 0,0
foreach ($line in $netstat) {
    if ($line -match "\s+(\d+)$") {
        $pid = $matches[1]
        try {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            Write-Host "  ✓ Proceso $pid liberado del puerto 7001" -ForegroundColor Green
        }
        catch {
            Write-Host "  ⚠ No se pudo detener proceso $pid" -ForegroundColor DarkYellow
        }
    }
}

$netstat = netstat -ano | Select-String ":5002" -Context 0,0
foreach ($line in $netstat) {
    if ($line -match "\s+(\d+)$") {
        $pid = $matches[1]
        try {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            Write-Host "  ✓ Proceso $pid liberado del puerto 5002" -ForegroundColor Green
        }
        catch {
            Write-Host "  ⚠ No se pudo detener proceso $pid" -ForegroundColor DarkYellow
        }
    }
}

Start-Sleep -Seconds 2

# Paso 3: dotnet clean
Write-Host "[3/8] Ejecutando dotnet clean..." -ForegroundColor Yellow
dotnet clean --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Clean completado" -ForegroundColor Green
} else {
    Write-Host "  ✗ Error en clean" -ForegroundColor Red
}

# Paso 4: Eliminar bin/obj (opcional, más agresivo)
Write-Host "[4/8] Eliminando carpetas bin y obj..." -ForegroundColor Yellow

$binFolders = Get-ChildItem -Path . -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue
$objFolders = Get-ChildItem -Path . -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue

$totalFolders = $binFolders.Count + $objFolders.Count

if ($totalFolders -gt 0) {
    foreach ($folder in $binFolders) {
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    foreach ($folder in $objFolders) {
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  ✓ $totalFolders carpetas eliminadas" -ForegroundColor Green
} else {
    Write-Host "  ✓ No se encontraron carpetas bin/obj" -ForegroundColor Green
}

# Paso 5: dotnet restore
Write-Host "[5/8] Ejecutando dotnet restore..." -ForegroundColor Yellow
dotnet restore --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Restore completado" -ForegroundColor Green
} else {
    Write-Host "  ✗ Error en restore" -ForegroundColor Red
}

# Paso 6: dotnet build
Write-Host "[6/8] Ejecutando dotnet build..." -ForegroundColor Yellow
dotnet build --no-restore --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Build completado exitosamente" -ForegroundColor Green
} else {
    Write-Host "  ✗ Error en build - revisar errores" -ForegroundColor Red
    exit 1
}

# Paso 7: Iniciar API
Write-Host "[7/8] Iniciando API en https://localhost:7001..." -ForegroundColor Yellow

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\AgoraHub360.ERP.Api'; dotnet run" -WindowStyle Normal
Start-Sleep -Seconds 3
Write-Host "  ✓ API iniciada" -ForegroundColor Green

# Paso 8: Iniciar Web
Write-Host "[8/8] Iniciando Web en https://localhost:5002..." -ForegroundColor Yellow

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\AgoraHub360.ERP.Web'; dotnet run" -WindowStyle Normal
Start-Sleep -Seconds 3
Write-Host "  ✓ Web iniciada" -ForegroundColor Green

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   REINICIO COMPLETADO" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servicios corriendo:" -ForegroundColor White
Write-Host "  • API:  https://localhost:7001" -ForegroundColor Cyan
Write-Host "  • Web:  https://localhost:5002" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANTE: En el navegador:" -ForegroundColor Yellow
Write-Host "  1. Abrir DevTools (F12)" -ForegroundColor White
Write-Host "  2. Application → Clear Storage → Clear site data" -ForegroundColor White
Write-Host "  3. Service Workers → Unregister (si aparece)" -ForegroundColor White
Write-Host "  4. Recargar con Ctrl+F5" -ForegroundColor White
Write-Host ""

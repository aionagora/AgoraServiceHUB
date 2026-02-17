# Script para testear la conexión a la base de datos de AgoraHub360 ERP
# Ejecutar desde la raíz del proyecto

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AgoraHub360 ERP - Database Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuración
$apiUrl = "https://localhost:7001"
$apiUrlHttp = "http://localhost:5000"

Write-Host "?? Probando conectividad con la API..." -ForegroundColor Yellow

# Test 1: Ping básico
Write-Host "`n1?? Test: API Ping" -ForegroundColor Green
try {
    $pingResponse = Invoke-RestMethod -Uri "$apiUrl/api/v1/diagnostics/ping" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "   ? API está activa" -ForegroundColor Green
    Write-Host "   Timestamp: $($pingResponse.timestamp)" -ForegroundColor Gray
    Write-Host "   Version: $($pingResponse.version)" -ForegroundColor Gray
} catch {
    Write-Host "   ? Error: $_" -ForegroundColor Red
    Write-Host "`n?? Asegúrate de que la API esté corriendo en $apiUrl" -ForegroundColor Yellow
    Write-Host "   Ejecuta: cd src/AgoraHub360.ERP.Api && dotnet run" -ForegroundColor Yellow
    exit 1
}

# Test 2: Health Check
Write-Host "`n2?? Test: Health Check" -ForegroundColor Green
try {
    $healthResponse = Invoke-RestMethod -Uri "$apiUrl/health" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "   ? Health Check: $($healthResponse.status)" -ForegroundColor Green
} catch {
    Write-Host "   ?? Health Check no disponible" -ForegroundColor Yellow
}

# Test 3: Database Test completo
Write-Host "`n3?? Test: Conexión a Base de Datos (Detallado)" -ForegroundColor Green
Write-Host "   Probando conectividad, permisos y datos..." -ForegroundColor Gray

try {
    $dbTestResponse = Invoke-RestMethod -Uri "$apiUrl/api/v1/diagnostics/database-test" -Method Get -SkipCertificateCheck -ErrorAction Stop
    
    Write-Host "`n   ?? Resultado General: $($dbTestResponse.status.ToUpper())" -ForegroundColor $(if ($dbTestResponse.status -eq "success") { "Green" } else { "Red" })
    Write-Host "   ?? Connection String: $($dbTestResponse.connectionString)" -ForegroundColor Gray
    Write-Host ""
    
    # Mostrar cada test
    foreach ($test in $dbTestResponse.tests) {
        $statusColor = switch -Wildcard ($test.status) {
            "*?*" { "Green" }
            "*??*" { "Yellow" }
            "*?*" { "Red" }
            default { "White" }
        }
        
        Write-Host "   $($test.test): $($test.status)" -ForegroundColor $statusColor
        Write-Host "      ? $($test.message)" -ForegroundColor Gray
        
        # Mostrar datos de monedas si existen
        if ($test.test -eq "Read Monedas Table" -and $test.data) {
            Write-Host "      Monedas encontradas:" -ForegroundColor DarkGray
            foreach ($moneda in $test.data) {
                Write-Host "         - [$($moneda.Codigo)] $($moneda.Nombre) ($($moneda.Simbolo))" -ForegroundColor DarkGray
            }
        }
        
        # Mostrar datos de roles si existen
        if ($test.test -eq "Read Roles Table" -and $test.data) {
            Write-Host "      Roles encontrados:" -ForegroundColor DarkGray
            foreach ($rol in $test.data) {
                Write-Host "         - [$($rol.Id)] $($rol.Nombre) - $($rol.Descripcion)" -ForegroundColor DarkGray
            }
        }
        
        # Mostrar migraciones pendientes si existen
        if ($test.test -eq "Database Migrations" -and $test.pending -and $test.pending.Count -gt 0) {
            Write-Host "      Migraciones pendientes:" -ForegroundColor Yellow
            foreach ($migration in $test.pending) {
                Write-Host "         - $migration" -ForegroundColor Yellow
            }
            Write-Host "      Para aplicar: dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api" -ForegroundColor Cyan
        }
        
        # Mostrar errores si existen
        if ($test.error) {
            Write-Host "      ?? Error: $($test.error)" -ForegroundColor Red
            if ($test.innerError) {
                Write-Host "      ?? Inner Error: $($test.innerError)" -ForegroundColor Red
            }
        }
        
        Write-Host ""
    }
    
    # Resumen
    if ($dbTestResponse.summary) {
        Write-Host "   ?? Resumen de Tests:" -ForegroundColor Cyan
        Write-Host "      Total: $($dbTestResponse.summary.totalTests)" -ForegroundColor White
        Write-Host "      ? Passed: $($dbTestResponse.summary.passed)" -ForegroundColor Green
        Write-Host "      ?? Warnings: $($dbTestResponse.summary.warnings)" -ForegroundColor Yellow
        Write-Host "      ? Failed: $($dbTestResponse.summary.failed)" -ForegroundColor Red
    }
    
    # Guardar resultado completo en JSON
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $outputFile = "database-test-result_$timestamp.json"
    $dbTestResponse | ConvertTo-Json -Depth 10 | Out-File -FilePath $outputFile -Encoding UTF8
    Write-Host "`n   ?? Resultado completo guardado en: $outputFile" -ForegroundColor Cyan
    
} catch {
    Write-Host "   ? Error al ejecutar database test: $_" -ForegroundColor Red
    Write-Host "   Detalles: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Tests Completados" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

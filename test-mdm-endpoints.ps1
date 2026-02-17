# Script de Verificación de Endpoints MDM
Write-Host "`n????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?                                                              ?" -ForegroundColor Cyan
Write-Host "?    ?? VERIFICACIÓN DE ENDPOINTS MDM                         ?" -ForegroundColor Cyan
Write-Host "?                                                              ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Verificar que la API esté corriendo
Write-Host "1??  Verificando que la API esté corriendo..." -ForegroundColor Yellow
Write-Host "   ?????????????????????????????????????????????" -ForegroundColor DarkGray

try {
    $healthCheck = Invoke-WebRequest -Uri "https://localhost:7001/health" `
        -SkipCertificateCheck `
        -TimeoutSec 5 `
        -ErrorAction Stop
    
    if ($healthCheck.StatusCode -eq 200) {
        Write-Host "   ? API está corriendo" -ForegroundColor Green
    }
}
catch {
    Write-Host "   ? API NO está corriendo" -ForegroundColor Red
    Write-Host "      Ejecuta: " -NoNewline -ForegroundColor Gray
    Write-Host ".\start-system.ps1" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

# Definir endpoints MDM
$endpoints = @(
    @{ Nombre = "UnidadesMedida"; Url = "https://localhost:7001/api/v1/unidades-medida"; Implementado = $true },
    @{ Nombre = "Almacenes"; Url = "https://localhost:7001/api/v1/almacenes"; Implementado = $true },
    @{ Nombre = "Categorías"; Url = "https://localhost:7001/api/v1/categorias"; Implementado = $false },
    @{ Nombre = "Clientes"; Url = "https://localhost:7001/api/v1/clientes"; Implementado = $false },
    @{ Nombre = "Proveedores"; Url = "https://localhost:7001/api/v1/proveedores"; Implementado = $false },
    @{ Nombre = "Productos"; Url = "https://localhost:7001/api/v1/productos"; Implementado = $false }
)

Write-Host "`n2??  Verificando endpoints MDM..." -ForegroundColor Yellow
Write-Host "   ?????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host ""

$implementados = 0
$total = $endpoints.Count

foreach ($endpoint in $endpoints) {
    Write-Host "   ?? $($endpoint.Nombre): " -NoNewline -ForegroundColor Cyan
    
    try {
        $response = Invoke-WebRequest -Uri $endpoint.Url `
            -SkipCertificateCheck `
            -TimeoutSec 3 `
            -ErrorAction Stop
        
        # Si llegamos aquí, el endpoint respondió con 200
        Write-Host "? " -NoNewline -ForegroundColor Green
        Write-Host "200 OK" -ForegroundColor White
        Write-Host "      URL: $($endpoint.Url)" -ForegroundColor Gray
        $implementados++
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq 401) {
            # 401 significa que el endpoint existe pero requiere autenticación
            Write-Host "? " -NoNewline -ForegroundColor Green
            Write-Host "401 Unauthorized (existe, requiere auth)" -ForegroundColor Yellow
            Write-Host "      URL: $($endpoint.Url)" -ForegroundColor Gray
            $implementados++
        }
        elseif ($statusCode -eq 404) {
            # 404 significa que el endpoint no está implementado
            Write-Host "? " -NoNewline -ForegroundColor Red
            Write-Host "404 Not Found (no implementado)" -ForegroundColor Red
            Write-Host "      URL: $($endpoint.Url)" -ForegroundColor Gray
            
            if (-not $endpoint.Implementado) {
                Write-Host "      ??  Esperado (aún no implementado)" -ForegroundColor DarkYellow
            }
        }
        elseif ($null -eq $statusCode) {
            # Error de conexión
            Write-Host "? " -NoNewline -ForegroundColor Red
            Write-Host "Error de conexión" -ForegroundColor Red
            Write-Host "      $($_.Exception.Message)" -ForegroundColor Gray
        }
        else {
            # Otro error
            Write-Host "??  " -NoNewline -ForegroundColor Yellow
            Write-Host "Error $statusCode" -ForegroundColor Yellow
            Write-Host "      URL: $($endpoint.Url)" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
}

# Resumen
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "?? RESUMEN" -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray

$porcentaje = [math]::Round(($implementados / $total) * 100)

Write-Host "`n   Endpoints implementados: " -NoNewline -ForegroundColor White
Write-Host "$implementados / $total" -ForegroundColor Cyan
Write-Host "   Progreso: " -NoNewline -ForegroundColor White
Write-Host "$porcentaje%" -ForegroundColor $(if ($porcentaje -eq 100) { "Green" } elseif ($porcentaje -ge 50) { "Yellow" } else { "Red" })

if ($implementados -eq $total) {
    Write-Host "`n   ? " -NoNewline -ForegroundColor Green
    Write-Host "Todos los endpoints MDM están implementados" -ForegroundColor Green
}
else {
    $faltantes = $total - $implementados
    Write-Host "`n   ??  " -NoNewline -ForegroundColor Yellow
    Write-Host "Faltan $faltantes endpoint(s) por implementar:" -ForegroundColor Yellow
    
    foreach ($endpoint in $endpoints) {
        if (-not $endpoint.Implementado) {
            Write-Host "      • $($endpoint.Nombre)" -ForegroundColor Gray
        }
    }
}

# Verificar Swagger
Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "?? SWAGGER UI" -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray

try {
    $swagger = Invoke-WebRequest -Uri "https://localhost:7001/swagger/index.html" `
        -SkipCertificateCheck `
        -TimeoutSec 3 `
        -ErrorAction Stop
    
    if ($swagger.StatusCode -eq 200) {
        Write-Host "`n   ? Swagger disponible en: " -NoNewline -ForegroundColor Green
        Write-Host "https://localhost:7001/swagger/index.html" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "`n   ??  Swagger no disponible" -ForegroundColor Yellow
}

# Recomendaciones
Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "?? PRÓXIMOS PASOS" -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray

if ($implementados -lt $total) {
    Write-Host "`n   1. Implementar controladores faltantes" -ForegroundColor White
    Write-Host "      Ver plantilla en: " -NoNewline -ForegroundColor Gray
    Write-Host "MDM-ENDPOINTS-VERIFICATION.md" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   2. Seguir el patrón de UnidadesMedidaController" -ForegroundColor White
    Write-Host ""
    Write-Host "   3. Usar rutas explícitas (no [controller])" -ForegroundColor White
    Write-Host "      Ejemplo: " -NoNewline -ForegroundColor Gray
    Write-Host '[Route("api/v{version:apiVersion}/categorias")]' -ForegroundColor Cyan
}
else {
    Write-Host "`n   1. Probar endpoints en Swagger" -ForegroundColor White
    Write-Host "   2. Verificar autenticación y multi-tenant" -ForegroundColor White
    Write-Host "   3. Implementar páginas Blazor para cada entidad" -ForegroundColor White
}

Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host ""

# Exportar resultados a archivo
$resultados = @{
    Fecha = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Implementados = $implementados
    Total = $total
    Porcentaje = $porcentaje
    Endpoints = $endpoints
}

$resultados | ConvertTo-Json -Depth 3 | Out-File "mdm-endpoints-status.json" -Encoding UTF8
Write-Host "?? Resultados exportados a: " -NoNewline -ForegroundColor Gray
Write-Host "mdm-endpoints-status.json" -ForegroundColor Cyan
Write-Host ""

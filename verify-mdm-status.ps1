# Script de Verificación del Estado MDM
Write-Host "`n????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?                                                              ?" -ForegroundColor Cyan
Write-Host "?    ?? VERIFICACIÓN DE ESTADO - MÓDULO MDM                   ?" -ForegroundColor Cyan
Write-Host "?                                                              ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$entidades = @("Cliente", "Proveedor", "Producto", "CategoriaProducto", "UnidadMedida", "Almacen")

Write-Host "?? VERIFICANDO COMPONENTES..." -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????`n" -ForegroundColor DarkGray

foreach ($entidad in $entidades) {
    Write-Host "?? $entidad" -ForegroundColor Cyan
    Write-Host "   ?????????????????????????????????????" -ForegroundColor DarkGray
    
    # Verificar Entidad de Dominio
    $dominioPath = "src\AgoraHub360.ERP.Domain\Entities\MDM\$entidad.cs"
    if (Test-Path $dominioPath) {
        Write-Host "   ? Entidad de Dominio" -ForegroundColor Green
    } else {
        Write-Host "   ? Entidad de Dominio" -ForegroundColor Red
    }
    
    # Verificar DTOs
    $dtoPath = "src\AgoraHub360.ERP.Shared\DTOs\$entidad"
    if (Test-Path $dtoPath) {
        $dtoDto = Test-Path "$dtoPath\$($entidad)Dto.cs"
        $createDto = Test-Path "$dtoPath\Create$($entidad)Dto.cs"
        $updateDto = Test-Path "$dtoPath\Update$($entidad)Dto.cs"
        
        if ($dtoDto -and $createDto -and $updateDto) {
            Write-Host "   ? DTOs Completos" -ForegroundColor Green
        } elseif ($dtoDto -or $createDto -or $updateDto) {
            Write-Host "   ??  DTOs Incompletos" -ForegroundColor Yellow
        } else {
            Write-Host "   ? DTOs" -ForegroundColor Red
        }
    } else {
        Write-Host "   ? DTOs" -ForegroundColor Red
    }
    
    # Verificar Interface de Servicio
    $interfacePath = "src\AgoraHub360.ERP.Application\Interfaces\I$($entidad)Service.cs"
    if (Test-Path $interfacePath) {
        Write-Host "   ? Interface de Servicio" -ForegroundColor Green
    } else {
        Write-Host "   ? Interface de Servicio" -ForegroundColor Red
    }
    
    # Verificar Servicio
    $servicePath = "src\AgoraHub360.ERP.Application\Services\$($entidad)Service.cs"
    if (Test-Path $servicePath) {
        Write-Host "   ? Servicio de Aplicación" -ForegroundColor Green
    } else {
        Write-Host "   ? Servicio de Aplicación" -ForegroundColor Red
    }
    
    # Verificar Controlador
    $controllerPath = "src\AgoraHub360.ERP.Api\Controllers\V1\$($entidad)sController.cs"
    if (Test-Path $controllerPath) {
        Write-Host "   ? Controlador API" -ForegroundColor Green
    } else {
        Write-Host "   ? Controlador API" -ForegroundColor Red
    }
    
    # Verificar HTTP Service
    $httpServicePath = "src\AgoraHub360.ERP.Web\Services\$($entidad)HttpService.cs"
    if (Test-Path $httpServicePath) {
        Write-Host "   ? HTTP Service (Blazor)" -ForegroundColor Green
    } else {
        Write-Host "   ? HTTP Service (Blazor)" -ForegroundColor Red
    }
    
    # Verificar Página Razor
    $razonNombre = if ($entidad -eq "CategoriaProducto") { "Categorias" } elseif ($entidad -eq "UnidadMedida") { "UnidadesMedida" } elseif ($entidad -eq "Almacen") { "Almacenes" } else { "$($entidad)s" }
    $razorPath = "src\AgoraHub360.ERP.Web\Pages\MDM\$razonNombre.razor"
    if (Test-Path $razorPath) {
        Write-Host "   ? Página Razor" -ForegroundColor Green
    } else {
        Write-Host "   ? Página Razor" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "?? RESUMEN:" -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray

$totalComponentes = $entidades.Count * 7
$componentesOk = 0

foreach ($entidad in $entidades) {
    if (Test-Path "src\AgoraHub360.ERP.Domain\Entities\MDM\$entidad.cs") { $componentesOk++ }
    if (Test-Path "src\AgoraHub360.ERP.Shared\DTOs\$entidad") { $componentesOk++ }
    if (Test-Path "src\AgoraHub360.ERP.Application\Interfaces\I$($entidad)Service.cs") { $componentesOk++ }
    if (Test-Path "src\AgoraHub360.ERP.Application\Services\$($entidad)Service.cs") { $componentesOk++ }
    if (Test-Path "src\AgoraHub360.ERP.Api\Controllers\V1\$($entidad)sController.cs") { $componentesOk++ }
    if (Test-Path "src\AgoraHub360.ERP.Web\Services\$($entidad)HttpService.cs") { $componentesOk++ }
    
    $razonNombre = if ($entidad -eq "CategoriaProducto") { "Categorias" } elseif ($entidad -eq "UnidadMedida") { "UnidadesMedida" } elseif ($entidad -eq "Almacen") { "Almacenes" } else { "$($entidad)s" }
    if (Test-Path "src\AgoraHub360.ERP.Web\Pages\MDM\$razonNombre.razor") { $componentesOk++ }
}

$porcentaje = [math]::Round(($componentesOk / $totalComponentes) * 100, 1)

Write-Host "   Componentes Implementados: $componentesOk / $totalComponentes" -ForegroundColor White
Write-Host "   Progreso: " -NoNewline -ForegroundColor White
Write-Host "$porcentaje%" -ForegroundColor $(if ($porcentaje -lt 30) { "Red" } elseif ($porcentaje -lt 70) { "Yellow" } else { "Green" })
Write-Host ""

Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "?? RECOMENDACIÓN:" -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray

if ($porcentaje -lt 30) {
    Write-Host "   ?? CRÍTICO: Comenzar implementación desde cero" -ForegroundColor Red
    Write-Host "   Sugerencia: Iniciar con UnidadMedida y CategoriaProducto" -ForegroundColor White
} elseif ($porcentaje -lt 70) {
    Write-Host "   ?? EN PROGRESO: Completar componentes faltantes" -ForegroundColor Yellow
    Write-Host "   Sugerencia: Priorizar servicios y controladores" -ForegroundColor White
} else {
    Write-Host "   ?? CASI COMPLETO: Finalizar últimos detalles" -ForegroundColor Green
    Write-Host "   Sugerencia: Testing y validación de UX" -ForegroundColor White
}

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "  ?? Ver: MDM-IMPLEMENTATION-PLAN.md para detalles completos" -ForegroundColor Gray
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host ""

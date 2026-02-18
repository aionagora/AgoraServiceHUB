# Script para verificar rutas de controladores MDM
Write-Host "`n?? Verificando rutas de controladores MDM..." -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????`n" -ForegroundColor DarkGray

$controladores = @(
    @{ Nombre = "UnidadesMedida"; Archivo = "src\AgoraHub360.ERP.Api\Controllers\V1\UnidadesMedidaController.cs"; RutaCorrecta = 'api/v{version:apiVersion}/unidades-medida' },
    @{ Nombre = "Categorias"; Archivo = "src\AgoraHub360.ERP.Api\Controllers\V1\CategoriasProductoController.cs"; RutaCorrecta = 'api/v{version:apiVersion}/categorias' },
    @{ Nombre = "Clientes"; Archivo = "src\AgoraHub360.ERP.Api\Controllers\V1\ClientesController.cs"; RutaCorrecta = 'api/v{version:apiVersion}/clientes' },
    @{ Nombre = "Proveedores"; Archivo = "src\AgoraHub360.ERP.Api\Controllers\V1\ProveedoresController.cs"; RutaCorrecta = 'api/v{version:apiVersion}/proveedores' },
    @{ Nombre = "Productos"; Archivo = "src\AgoraHub360.ERP.Api\Controllers\V1\ProductosController.cs"; RutaCorrecta = 'api/v{version:apiVersion}/productos' },
    @{ Nombre = "Almacenes"; Archivo = "src\AgoraHub360.ERP.Api\Controllers\V1\AlmacenesController.cs"; RutaCorrecta = 'api/v{version:apiVersion}/almacenes' }
)

$problemas = 0

foreach ($ctrl in $controladores) {
    Write-Host "?? $($ctrl.Nombre):" -ForegroundColor Cyan -NoNewline
    
    if (Test-Path $ctrl.Archivo) {
        $contenido = Get-Content $ctrl.Archivo -Raw
        
        # Buscar la línea de Route
        if ($contenido -match '\[Route\("(.+?)"\)\]') {
            $rutaActual = $matches[1]
            
            if ($rutaActual -eq $ctrl.RutaCorrecta) {
                Write-Host " ? " -ForegroundColor Green -NoNewline
                Write-Host $rutaActual -ForegroundColor Gray
            }
            elseif ($rutaActual -like "*[controller]*") {
                Write-Host " ? " -ForegroundColor Red -NoNewline
                Write-Host "Usa [controller] (debe ser explícita)" -ForegroundColor Red
                Write-Host "      Ruta actual:   $rutaActual" -ForegroundColor Gray
                Write-Host "      Ruta correcta: $($ctrl.RutaCorrecta)" -ForegroundColor Green
                $problemas++
            }
            else {
                Write-Host " ??  " -ForegroundColor Yellow -NoNewline
                Write-Host "Ruta diferente a la esperada" -ForegroundColor Yellow
                Write-Host "      Ruta actual:   $rutaActual" -ForegroundColor Gray
                Write-Host "      Ruta esperada: $($ctrl.RutaCorrecta)" -ForegroundColor Green
                $problemas++
            }
        }
        else {
            Write-Host " ??  No se encontró [Route]" -ForegroundColor Yellow
            $problemas++
        }
    }
    else {
        Write-Host " ??  Controlador no existe (aún no implementado)" -ForegroundColor Gray
    }
}

Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "?? RESUMEN:" -ForegroundColor Yellow
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor DarkGray

if ($problemas -eq 0) {
    Write-Host "`n   ? " -NoNewline -ForegroundColor Green
    Write-Host "Todos los controladores tienen rutas correctas" -ForegroundColor Green
}
else {
    Write-Host "`n   ? " -NoNewline -ForegroundColor Red
    Write-Host "Se encontraron $problemas problema(s) de rutas" -ForegroundColor Red
    Write-Host "`n   ?? Recomendación:" -ForegroundColor Yellow
    Write-Host "      Cambiar [Route(\"api/v{version:apiVersion}/[controller]\")]" -ForegroundColor Gray
    Write-Host "      Por:     [Route(\"api/v{version:apiVersion}/nombre-explicito\")]" -ForegroundColor Green
}

Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host ""

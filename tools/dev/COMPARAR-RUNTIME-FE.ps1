# ================================================
# COMPARADOR DE RUNTIME — PC Problemática vs PC Base
# ================================================
# Ejecutar en la PC problemática, en la carpeta raíz del proyecto.
# PASO 1: Asegurar que el DiagnosticoController existe en
#          src/AgoraHub360.ERP.Api/Controllers/V1/DiagnosticoController.cs
#          Si no existe, copiarlo desde la PC base o recrearlo del REPORTE-FORENSE-FE.md
#
# PASO 2: dotnet build
# PASO 3: dotnet run --project src/AgoraHub360.ERP.Api --launch-profile https
# PASO 4: En OTRA terminal, ejecutar este script
# ================================================

$ErrorActionPreference = "Stop"
$BaseUrl = "https://localhost:7001"
$BaseFileRuntime = ".\DIAG-RUNTIME-PC-BASE.json"
$BaseFileFE = ".\DIAG-FE-PC-BASE.json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "COMPARADOR DE RUNTIME FE" -ForegroundColor Cyan
Write-Host "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "PC: $env:COMPUTERNAME" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ---- TEST 1: API RESPONDE ----
Write-Host "`n[TEST 1] ¿API responde? " -NoNewline
try {
    $health = Invoke-RestMethod -Uri "$BaseUrl/health" -SkipCertificateCheck
    Write-Host "SI ($health)" -ForegroundColor Green
} catch {
    Write-Host "NO - $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ---- TEST 2: RUNTIME DIAGNOSTIC ----
Write-Host "`n[TEST 2] Diagnóstico Runtime" -ForegroundColor Yellow
try {
    $runtime = Invoke-RestMethod -Uri "$BaseUrl/api/v1/diagnostico/runtime" -SkipCertificateCheck
    $baseRuntime = Get-Content $BaseFileRuntime -Raw | ConvertFrom-Json

    $compareFields = @(
        @{Field="MachineName"; Label="Hostname"},
        @{Field="AssemblyInformationalVersion"; Label="Commit/Version"},
        @{Field="ConnectionDatabase"; Label="Base de datos"},
        @{Field="ConnectionServer"; Label="Servidor SQL"},
        @{Field="EnvironmentName"; Label="Environment"},
        @{Field="DatabaseName"; Label="DB_NAME()"},
        @{Field="ServerName"; Label="@@SERVERNAME"},
        @{Field="ContentRootPath"; Label="Ruta API"},
        @{Field="ConnectionString"; Label="ConnectionString (oculto)"}
    )

    foreach ($f in $compareFields) {
        $baseVal = $baseRuntime.$($f.Field)
        $thisVal = $runtime.$($f.Field)
        if ($baseVal -eq $thisVal) {
            Write-Host "  ✅ $($f.Label): $thisVal" -ForegroundColor Green
        } else {
            Write-Host "  ❌ $($f.Label): DIFERENTE!" -ForegroundColor Red
            Write-Host "     PC BASE: $baseVal" -ForegroundColor Gray
            Write-Host "     PC ESTA: $thisVal" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# ---- TEST 3: FE DIAGNOSTIC ----
Write-Host "`n[TEST 3] Diagnóstico Facturación Electrónica" -ForegroundColor Yellow
try {
    $fe = Invoke-RestMethod -Uri "$BaseUrl/api/v1/diagnostico/facturacion-electronica" -SkipCertificateCheck
    $baseFe = Get-Content $BaseFileFE -Raw | ConvertFrom-Json

    # 3a. EF Mapping
    Write-Host "  --- EF Runtime Mapping ---" -ForegroundColor Cyan
    $baseMapping = $baseFe.EfRuntimeMapping
    $thisMapping = $fe.EfRuntimeMapping
    for ($i = 0; $i -lt $baseMapping.Count; $i++) {
        $bm = $baseMapping[$i]
        $tm = $thisMapping[$i]
        if ($bm.Schema -eq $tm.Schema -and $bm.Table -eq $tm.Table) {
            Write-Host "  ✅ $($bm.Entity): [$($bm.Schema)].[$($bm.Table)]" -ForegroundColor Green
        } else {
            Write-Host "  ❌ $($bm.Entity): DIFERENTE!" -ForegroundColor Red
            Write-Host "     PC BASE: [$($bm.Schema)].[$($bm.Table)]"
            Write-Host "     PC ESTA: [$($tm.Schema)].[$($tm.Table)]"
        }
    }

    # 3b. Record counts comparison
    Write-Host "  --- Conteo de Registros ---" -ForegroundColor Cyan
    $baseCounts = $baseFe.RecordCounts
    $thisCounts = $fe.RecordCounts
    for ($i = 0; $i -lt $baseCounts.Count; $i++) {
        $bc = $baseCounts[$i]
        $tc = $thisCounts[$i]
        if ($bc.count -eq $tc.count) {
            Write-Host "  ✅ $($bc.table): $($tc.count)" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️ $($bc.table): DIFERENTE!" -ForegroundColor Yellow
            Write-Host "     PC BASE: $($bc.count)"
            Write-Host "     PC ESTA: $($tc.count)"
        }
    }

    # 3c. Config activa
    Write-Host "  --- Configuración FE Activa ---" -ForegroundColor Cyan
    $baseCfg = $baseFe.ConfiguracionActiva
    $thisCfg = $fe.ConfiguracionActiva
    if ($baseCfg.Count -eq 0 -and $thisCfg.Count -eq 0) {
        Write-Host "  ⚠️ NO HAY CONFIGURACIÓN ACTIVA en ninguna PC" -ForegroundColor Yellow
    } elseif ($baseCfg.Count -eq 0) {
        Write-Host "  ❌ PC BASE sin configuración, PC ESTA si tiene" -ForegroundColor Red
        Write-Host "  ESTA: EmpresaId=$($thisCfg[0].EmpresaId), Proveedor=$($thisCfg[0].ProveedorCodigo)"
    } elseif ($thisCfg.Count -eq 0) {
        Write-Host "  ❌ PC ESTA sin configuración activa!" -ForegroundColor Red
        Write-Host "  PC BASE: EmpresaId=$($baseCfg[0].EmpresaId), Proveedor=$($baseCfg[0].ProveedorCodigo)"
    } else {
        $bc = $baseCfg[0]; $tc = $thisCfg[0]
        $cfgFields = @("EmpresaId", "ProveedorCodigo", "AmbienteCodigo", "NitEmisor", "ActivityCode")
        foreach ($f in $cfgFields) {
            if ($bc.$f -eq $tc.$f) {
                Write-Host "  ✅ $f = $($tc.$f)" -ForegroundColor Green
            } else {
                Write-Host "  ❌ $f: BASE=$($bc.$f) vs ESTA=$($tc.$f)" -ForegroundColor Red
            }
        }
    }

    # 3d. Latest Facturas
    Write-Host "  --- Últimas 3 FacturasVenta ---" -ForegroundColor Cyan
    $baseFacts = $baseFe.LatestFacturasVenta
    $thisFacts = $fe.LatestFacturasVenta
    $max = [Math]::Min(3, [Math]::Min($baseFacts.Count, $thisFacts.Count))
    for ($i = 0; $i -lt $max; $i++) {
        Write-Host "  ID=$($baseFacts[$i].Id) vs ID=$($thisFacts[$i].Id) - Nros: $($baseFacts[$i].NumeroFactura) vs $($thisFacts[$i].NumeroFactura)"
    }
    if ($baseFacts.Count -ne $thisFacts.Count) {
        Write-Host "  ⚠️ DIFERENCIA en cantidad: BASE=$($baseFacts.Count) ESTA=$($thisFacts.Count)" -ForegroundColor Yellow
    }
    if ($baseFacts.Count -gt 0 -and $thisFacts.Count -gt 0) {
        if ($baseFacts[0].Id -ne $thisFacts[0].Id) {
            Write-Host "  ⚠️ Último ID de factura DIFERENTE" -ForegroundColor Yellow
        }
    }

    # 3e. Auditoría
    Write-Host "  --- Auditoría FE ---" -ForegroundColor Cyan
    $baseAudit = $baseFe.LatestAuditoriaFacturacion
    $thisAudit = $fe.LatestAuditoriaFacturacion
    Write-Host "  BASE: $($baseAudit.Count) registros | ESTA: $($thisAudit.Count) registros"
    if ($baseAudit.Count -ne $thisAudit.Count) {
        Write-Host "  ⚠️ DIFERENCIA en cantidad de auditorías" -ForegroundColor Yellow
    }

    # 3f. Migrations
    Write-Host "  --- Migrations ---" -ForegroundColor Cyan
    $baseMigs = $baseFe.Migrations
    $thisMigs = $fe.Migrations
    if ($baseMigs[0].PSObject.Properties.Name -contains "Error" -and $thisMigs[0].PSObject.Properties.Name -contains "Error") {
        Write-Host "  ✅ Ambas PCs: sin __EFMigrationsHistory" -ForegroundColor Green
    } elseif ($baseMigs[0].PSObject.Properties.Name -contains "Error") {
        Write-Host "  ❌ PC BASE sin migrations, PC ESTA SI tiene" -ForegroundColor Red
    } elseif ($thisMigs[0].PSObject.Properties.Name -contains "Error") {
        Write-Host "  ❌ PC ESTA sin migrations, PC BASE SI tiene" -ForegroundColor Red
    } else {
        Write-Host "  ✅ Ambas tienen migrations history" -ForegroundColor Green
    }

} catch {
    Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# ---- TEST 4: TABLAS LEGACY ----
Write-Host "`n[TEST 4] Tablas legacy [dbo]" -ForegroundColor Yellow
try {
    $fe2 = $fe
    $dboExists = $fe2.TableExistenceInDb | Where-Object { $_.Schema -eq "dbo" -and $_.Table -eq "FacturasVenta" }
    $dboCount = $fe2.RecordCounts | Where-Object { $_.table -eq "dbo.FacturasVenta (legacy)" }
    if ($dboExists.Exists) {
        Write-Host "  ⚠️ [dbo].[FacturasVenta] EXISTE con $($dboCount.count) registros" -ForegroundColor Yellow
        if ($dboCount.count -gt 0) {
            Write-Host "  ❌ CONTIENE DATOS! Verificar si el flujo FE la está usando." -ForegroundColor Red
        } else {
            Write-Host "  ✅ Vacía (0 registros) - solo legacy, no interfiere" -ForegroundColor Green
        }
    } else {
        Write-Host "  ✅ [dbo].[FacturasVenta] NO EXISTE" -ForegroundColor Green
    }
} catch {
    Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "COMPARACIÓN COMPLETADA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Cualquier línea con ❌ o ⚠️ indica una diferencia entre la PC base y la PC problemática."
Write-Host "Si todas las líneas son ✅, el problema no está en el runtime - es CACHÉ DEL NAVEGADOR."

# Script: fix-tenant-required.ps1
# Propósito: Corregir el error 403 Tenant Required

Write-Host "`n???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? Corrección: Tenant Required (403)" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? Este script:" -ForegroundColor Yellow
Write-Host "   1. Verifica la asignación de empresa del usuario admin" -ForegroundColor White
Write-Host "   2. Corrige la base de datos si es necesario" -ForegroundColor White
Write-Host "   3. Genera un nuevo token con EmpresaId" -ForegroundColor White
Write-Host "   4. Verifica que el token contenga el claim EmpresaId" -ForegroundColor White
Write-Host ""

# ??????????????????????????????????????????????????????????????
# Función: Ejecutar SQL directamente sin módulo
# ??????????????????????????????????????????????????????????????
function Invoke-SqlQuery {
    param(
        [string]$ServerInstance,
        [string]$Database,
        [string]$Username,
        [string]$Password,
        [string]$Query
    )
    
    try {
        $connectionString = "Server=$ServerInstance;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=true;Encrypt=false;"
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null
        
        $connection.Close()
        
        return $dataset.Tables[0]
    }
    catch {
        Write-Host "   ? Error SQL: $_" -ForegroundColor Red
        return $null
    }
}

# ??????????????????????????????????????????????????????????????
# Paso 1: Verificar y corregir BD
# ??????????????????????????????????????????????????????????????
Write-Host "1?? Verificando base de datos..." -ForegroundColor Yellow
Write-Host ""

# IMPORTANTE: Ajusta estos valores según tu configuración
$server = "localhost,1433"
$database = "AgoraHub360_ERP_Dev"
$username = "sa"
$password = Read-Host "Ingresa la contraseña de SQL Server" -AsSecureString
$passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

$sqlQuery = @"
-- Verificar estado actual
SELECT 
    u.Id AS UsuarioId,
    u.NombreUsuario,
    u.Email,
    u.EmpresaActivaId,
    ue.EmpresaId AS EmpresaAsignada,
    ue.Rol
FROM core.Usuarios u
LEFT JOIN core.UsuarioEmpresas ue ON u.Id = ue.UsuarioId
WHERE u.Email = 'admin@agorahub360.com';
"@

$result = Invoke-SqlQuery -ServerInstance $server -Database $database -Username $username -Password $passwordPlain -Query $sqlQuery

if ($result) {
    Write-Host "   ?? Estado actual:" -ForegroundColor Cyan
    $result | Format-Table -AutoSize
    
    $empresaAsignada = $result.Rows[0]["EmpresaAsignada"]
    $empresaActiva = $result.Rows[0]["EmpresaActivaId"]
    
    if ([DBNull]::Value.Equals($empresaAsignada) -or $null -eq $empresaAsignada) {
        Write-Host "   ??  Usuario SIN empresa asignada. Corrigiendo..." -ForegroundColor Yellow
        
        $fixQuery = @"
-- Asignar empresa al usuario
IF NOT EXISTS (SELECT 1 FROM core.UsuarioEmpresas WHERE UsuarioId = 1 AND EmpresaId = 1)
BEGIN
    INSERT INTO core.UsuarioEmpresas (UsuarioId, EmpresaId, Rol)
    VALUES (1, 1, 'Admin');
    SELECT 'Empresa asignada' AS Resultado;
END
ELSE
BEGIN
    SELECT 'Ya estaba asignada' AS Resultado;
END

-- Actualizar empresa activa
UPDATE core.Usuarios
SET EmpresaActivaId = 1
WHERE Id = 1 AND EmpresaActivaId IS NULL;

-- Verificar resultado
SELECT 
    u.Id,
    u.NombreUsuario,
    u.EmpresaActivaId,
    ue.EmpresaId,
    ue.Rol
FROM core.Usuarios u
INNER JOIN core.UsuarioEmpresas ue ON u.Id = ue.UsuarioId
WHERE u.Id = 1;
"@
        
        $fixResult = Invoke-SqlQuery -ServerInstance $server -Database $database -Username $username -Password $passwordPlain -Query $fixQuery
        
        if ($fixResult) {
            Write-Host "   ? Corrección aplicada" -ForegroundColor Green
            Write-Host ""
            Write-Host "   ?? Estado actualizado:" -ForegroundColor Cyan
            $fixResult | Format-Table -AutoSize
        }
    }
    elseif ([DBNull]::Value.Equals($empresaActiva) -or $null -eq $empresaActiva) {
        Write-Host "   ??  Usuario tiene empresa asignada pero no activa. Corrigiendo..." -ForegroundColor Yellow
        
        $updateQuery = "UPDATE core.Usuarios SET EmpresaActivaId = 1 WHERE Id = 1;"
        Invoke-SqlQuery -ServerInstance $server -Database $database -Username $username -Password $passwordPlain -Query $updateQuery | Out-Null
        
        Write-Host "   ? EmpresaActivaId actualizada" -ForegroundColor Green
    }
    else {
        Write-Host "   ? Usuario correctamente configurado" -ForegroundColor Green
    }
}
else {
    Write-Host "   ? No se pudo conectar a la base de datos" -ForegroundColor Red
    Write-Host "   Verifica la cadena de conexión y ejecuta manualmente:" -ForegroundColor Yellow
    Write-Host "   $sqlQuery" -ForegroundColor Gray
    exit
}

Write-Host ""

# ??????????????????????????????????????????????????????????????
# Paso 2: Generar nuevo token
# ??????????????????????????????????????????????????????????????
Write-Host "2?? Generando nuevo token JWT..." -ForegroundColor Yellow
Write-Host ""

try {
    $loginData = @{
        email = "admin@agorahub360.com"
        password = "Admin123"
    } | ConvertTo-Json

    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    
    $loginResponse = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/auth/login" `
        -Method Post `
        -Body $loginData `
        -ContentType "application/json" `
        -ErrorAction Stop

    if ($loginResponse.success) {
        $token = $loginResponse.data.token
        
        # Decodificar payload del JWT
        $payload = $token.Split('.')[1]
        # Agregar padding si es necesario
        $paddedPayload = $payload
        $mod = $payload.Length % 4
        if ($mod -gt 0) {
            $paddedPayload += ('=' * (4 - $mod))
        }
        
        $decodedBytes = [Convert]::FromBase64String($paddedPayload)
        $decodedJson = [System.Text.Encoding]::UTF8.GetString($decodedBytes)
        $claims = $decodedJson | ConvertFrom-Json
        
        Write-Host "   ? Token generado exitosamente" -ForegroundColor Green
        Write-Host ""
        Write-Host "   ?? Claims del token:" -ForegroundColor Cyan
        Write-Host "      • NameIdentifier: $($claims.nameid)" -ForegroundColor White
        Write-Host "      • Name: $($claims.unique_name)" -ForegroundColor White
        Write-Host "      • Email: $($claims.email)" -ForegroundColor White
        Write-Host "      • Role: $($claims.role)" -ForegroundColor White
        
        if ($claims.EmpresaId) {
            Write-Host "      • EmpresaId: $($claims.EmpresaId)" -ForegroundColor Green
            Write-Host ""
            Write-Host "   ? El token CONTIENE EmpresaId" -ForegroundColor Green
            Write-Host "   El error 403 debería estar resuelto" -ForegroundColor Green
        } else {
            Write-Host "      • EmpresaId: [NO PRESENTE]" -ForegroundColor Red
            Write-Host ""
            Write-Host "   ? El token AÚN NO contiene EmpresaId" -ForegroundColor Red
            Write-Host "   Verifica manualmente la base de datos" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "   ? Login fallido: $($loginResponse.message)" -ForegroundColor Red
    }
}
catch {
    Write-Host "   ? Error al hacer login: $_" -ForegroundColor Red
    Write-Host "   Verifica que la API esté corriendo en puerto 7001" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Siguiente paso:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   1. Abre la aplicación web: " -NoNewline -ForegroundColor White
Write-Host "https://localhost:5002" -ForegroundColor Cyan
Write-Host ""
Write-Host "   2. Si ya estás logueado:" -ForegroundColor White
Write-Host "      • Haz clic en tu nombre (esquina superior derecha)" -ForegroundColor Gray
Write-Host "      • Haz clic en 'Cerrar Sesión'" -ForegroundColor Gray
Write-Host ""
Write-Host "   3. Vuelve a iniciar sesión:" -ForegroundColor White
Write-Host "      • Email: admin@agorahub360.com" -ForegroundColor Gray
Write-Host "      • Contraseña: Admin123" -ForegroundColor Gray
Write-Host ""
Write-Host "   4. Intenta crear un parámetro nuevamente" -ForegroundColor White
Write-Host ""
Write-Host "   5. Verifica el token en la consola del navegador (F12):" -ForegroundColor White
Write-Host "      const token = localStorage.getItem('agorahub360_auth_token');" -ForegroundColor Cyan
Write-Host "      const payload = JSON.parse(atob(token.split('.')[1]));" -ForegroundColor Cyan
Write-Host "      console.log('EmpresaId:', payload.EmpresaId);" -ForegroundColor Cyan
Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Limpiar password de memoria
$passwordPlain = $null

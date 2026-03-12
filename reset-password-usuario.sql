-- ================================================================
-- SCRIPT: RESET DE CONTRASEÑA DE USUARIO
-- Sistema: AgoraHub360 ERP
-- Algoritmo: SHA256 ? Base64 (mismo que AuthService.cs)
--
-- PASO 1: Genera el hash de tu nueva contraseña con PowerShell:
--
--   $pwd = "TuNuevaContraseña"
--   $bytes = [System.Text.Encoding]::UTF8.GetBytes($pwd)
--   $sha = [System.Security.Cryptography.SHA256]::Create()
--   $hash = $sha.ComputeHash($bytes)
--   [Convert]::ToBase64String($hash)
--
-- PASO 2: Copia el resultado y pégalo en @NuevoHash abajo.
-- PASO 3: Indica el email o NombreUsuario del usuario a resetear.
-- PASO 4: Ejecuta el script en SQL Server Management Studio.
-- ================================================================

USE [db_AgoraERP_Core];
GO

-- ???????????????????????????????????????????????????????????????
-- ?  ? EDITA ESTAS DOS VARIABLES ANTES DE EJECUTAR             ?
-- ???????????????????????????????????????????????????????????????
DECLARE @NombreUsuario NVARCHAR(100) = 'admin';          -- ? usuario a resetear
DECLARE @NuevoHash     NVARCHAR(500) = 'PEGA_AQUI_EL_HASH'; -- ? hash generado en PowerShell

-- ================================================================
-- NO MODIFICAR A PARTIR DE AQUÍ
-- ================================================================
BEGIN TRANSACTION;

BEGIN TRY

    IF @NuevoHash = 'PEGA_AQUI_EL_HASH'
    BEGIN
        RAISERROR('ERROR: Debes reemplazar @NuevoHash con el hash real generado en PowerShell.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM [core].[Usuarios] WHERE NombreUsuario = @NombreUsuario)
    BEGIN
        RAISERROR('ERROR: No se encontró el usuario "%s".', 16, 1, @NombreUsuario);
        RETURN;
    END

    UPDATE [core].[Usuarios]
    SET    PasswordHash = @NuevoHash
    WHERE  NombreUsuario = @NombreUsuario;

    COMMIT TRANSACTION;

    PRINT '? Contraseña actualizada correctamente para: ' + @NombreUsuario;
    PRINT '';

    -- Verificación
    SELECT
        U.Id,
        U.NombreUsuario,
        U.Email,
        U.NombreCompleto,
        U.Activo,
        E.Nombre AS EmpresaActiva,
        UE.Rol
    FROM [core].[Usuarios] U
    LEFT JOIN [core].[Empresas]       E  ON U.EmpresaActivaId = E.Id
    LEFT JOIN [core].[UsuarioEmpresas] UE ON UE.UsuarioId = U.Id
    WHERE U.NombreUsuario = @NombreUsuario;

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '? ERROR: ' + ERROR_MESSAGE();
    THROW;
END CATCH
GO

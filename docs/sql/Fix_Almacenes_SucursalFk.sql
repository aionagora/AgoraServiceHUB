-- Fix_Almacenes_SucursalFk.sql
-- Propósito: Detectar y corregir registros "huérfanos" en mdm.Almacenes.SucursalId y recrear la FK con ON DELETE SET NULL.
-- IMPORTANTE: Hacer backup completo de la base de datos antes de ejecutar este script.
-- Ejecutar en entorno de staging primero y verificar resultados.

SET NOCOUNT ON;

BEGIN TRY
    PRINT '*** Inicio del script: Fix_Almacenes_SucursalFk.sql';

    PRINT '1) Mostrar conteo de registros huérfanos (Almacenes.SucursalId no existe en Sucursales)';
    SELECT COUNT(*) AS Huérfanos
    FROM mdm.Almacenes a
    LEFT JOIN core.Sucursales s ON a.SucursalId = s.Id
    WHERE s.Id IS NULL;

    PRINT '2) Mostrar ejemplo (TOP 100) de filas huérfanas';
    SELECT TOP(100) a.*
    FROM mdm.Almacenes a
    LEFT JOIN core.Sucursales s ON a.SucursalId = s.Id
    WHERE s.Id IS NULL
    ORDER BY a.Id;

    PRINT '--- Iniciando transacción para corrección';
    BEGIN TRANSACTION;

    -- 3) Buscar FK existente sobre mdm.Almacenes
    DECLARE @fkName sysname;
    SELECT @fkName = fk.name
    FROM sys.foreign_keys fk
    WHERE fk.parent_object_id = OBJECT_ID('mdm.Almacenes')
      AND fk.referenced_object_id = OBJECT_ID('core.Sucursales');

    IF @fkName IS NOT NULL
    BEGIN
        PRINT '-> Se encontró FK: ' + @fkName + ' - eliminando para poder modificar columna.';
        DECLARE @dropSql nvarchar(max) = N'ALTER TABLE mdm.Almacenes DROP CONSTRAINT [' + @fkName + N']';
        EXEC sp_executesql @dropSql;
    END
    ELSE
    BEGIN
        PRINT '-> No se encontró FK existente con referencia a core.Sucursales. Continuando.';
    END

    -- 4) Hacer la columna nullable
    PRINT '-> Alterando columna SucursalId a NULLABLE';
    ALTER TABLE mdm.Almacenes ALTER COLUMN SucursalId INT NULL;

    -- 5) Limpiar registros huérfanos: set NULL
    PRINT '-> Actualizando registros huérfanos: SET SucursalId = NULL';
    UPDATE mdm.Almacenes
    SET SucursalId = NULL
    WHERE SucursalId IS NOT NULL
      AND SucursalId NOT IN (SELECT Id FROM core.Sucursales);

    PRINT '-> Filas afectadas por la limpieza:';
    SELECT @@ROWCOUNT AS FilasActualizadas;

    -- 6) Crear FK con ON DELETE SET NULL
    PRINT '-> Creando FK: FK_Almacenes_Sucursales_SucursalId con ON DELETE SET NULL';
    ALTER TABLE mdm.Almacenes
    ADD CONSTRAINT FK_Almacenes_Sucursales_SucursalId
        FOREIGN KEY (SucursalId) REFERENCES core.Sucursales(Id)
        ON DELETE SET NULL;

    COMMIT TRANSACTION;
    PRINT '*** Transacción aplicada y commit realizado.';

    PRINT '7) Validación final: conteo de huérfanos (debe ser 0)';
    SELECT COUNT(*) AS HuérfanosPost
    FROM mdm.Almacenes a
    LEFT JOIN core.Sucursales s ON a.SucursalId = s.Id
    WHERE s.Id IS NULL;

    PRINT '*** Script finalizado correctamente.';
END TRY
BEGIN CATCH
    PRINT '*** ERROR durante la ejecución. Se ejecutará ROLLBACK.';
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END

    DECLARE @ErrorNumber int = ERROR_NUMBER();
    DECLARE @ErrorMessage nvarchar(4000) = ERROR_MESSAGE();
    DECLARE @ErrorState int = ERROR_STATE();
    PRINT CONCAT('ERROR_NUMBER: ', @ErrorNumber);
    PRINT CONCAT('ERROR_STATE: ', @ErrorState);
    PRINT CONCAT('ERROR_MESSAGE: ', @ErrorMessage);

    THROW;
END CATCH
;

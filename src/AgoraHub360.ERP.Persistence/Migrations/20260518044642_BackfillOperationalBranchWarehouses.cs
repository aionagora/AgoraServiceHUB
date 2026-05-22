using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillOperationalBranchWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill: Crear almacenes automáticos para sucursales operativas sin almacén
            migrationBuilder.Sql(@"
                INSERT INTO mdm.Almacenes (
                    EmpresaId, 
                    SucursalId, 
                    Codigo, 
                    Nombre, 
                    Activo, 
                    FechaCreacion, 
                    CreadoPor
                )
                SELECT 
                    s.EmpresaId,
                    s.Id,
                    'ALM-' + s.Codigo AS Codigo,
                    'Almacén ' + s.Nombre AS Nombre,
                    1 AS Activo,
                    GETDATE() AS FechaCreacion,
                    'SYSTEM-BACKFILL' AS CreadoPor
                FROM core.Sucursales s
                WHERE s.Activo = 1 
                  AND (s.ManejaAlmacen = 1 OR s.PermiteInventario = 1 OR s.PermiteDespacho = 1)
                  AND NOT EXISTS (
                      SELECT 1 
                      FROM mdm.Almacenes a 
                      WHERE a.SucursalId = s.Id AND a.Activo = 1
                  )
                  AND NOT EXISTS (
                      SELECT 1 
                      FROM mdm.Almacenes a2 
                      WHERE a2.EmpresaId = s.EmpresaId 
                        AND a2.Codigo = 'ALM-' + s.Codigo
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Eliminar almacenes creados automáticamente por el backfill
            migrationBuilder.Sql(@"
                DELETE FROM mdm.Almacenes 
                WHERE CreadoPor = 'SYSTEM-BACKFILL';
            ");
        }
    }
}

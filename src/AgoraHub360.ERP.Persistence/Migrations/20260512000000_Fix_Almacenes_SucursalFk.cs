using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    public partial class Fix_Almacenes_SucursalFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Drop existing FK if present. Adjust name if DB uses different FK name.
            try
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_Almacenes_Sucursales_SucursalId",
                    table: "Almacenes",
                    schema: "core");
            }
            catch
            {
                // If FK not found, ignore and continue. Some providers throw when not exists.
            }

            // 2) Make SucursalId nullable so we can set NULL for orphan records
            migrationBuilder.AlterColumn<int>(
                name: "SucursalId",
                schema: "core",
                table: "Almacenes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // 3) Clean orphan records: set SucursalId = NULL where referent does not exist
            migrationBuilder.Sql(@"
                UPDATE core.Almacenes
                SET SucursalId = NULL
                WHERE SucursalId IS NOT NULL
                  AND SucursalId NOT IN (SELECT Id FROM core.Sucursales)
            ");

            // 4) Recreate FK with ON DELETE SET NULL
            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "core",
                table: "Almacenes",
                column: "SucursalId",
                principalSchema: "core",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the FK we created
            try
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_Almacenes_Sucursales_SucursalId",
                    table: "Almacenes",
                    schema: "core");
            }
            catch
            {
            }

            // Revert column to non-nullable. Note: this may fail if NULL values exist.
            migrationBuilder.AlterColumn<int>(
                name: "SucursalId",
                schema: "core",
                table: "Almacenes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Recreate FK with Restrict (no action) to approximate previous behaviour
            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "core",
                table: "Almacenes",
                column: "SucursalId",
                principalSchema: "core",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlmacenSucursalNavegacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "mdm",
                table: "Almacenes");

            migrationBuilder.AlterColumn<int>(
                name: "SucursalId",
                schema: "mdm",
                table: "Almacenes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                schema: "mdm",
                table: "Almacenes",
                column: "EmpresaId",
                principalSchema: "core",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "mdm",
                table: "Almacenes",
                column: "SucursalId",
                principalSchema: "core",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                schema: "mdm",
                table: "Almacenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "mdm",
                table: "Almacenes");

            migrationBuilder.AlterColumn<int>(
                name: "SucursalId",
                schema: "mdm",
                table: "Almacenes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "mdm",
                table: "Almacenes",
                column: "SucursalId",
                principalSchema: "core",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

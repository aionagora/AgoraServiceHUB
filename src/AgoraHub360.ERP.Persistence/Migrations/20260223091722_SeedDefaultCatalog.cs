using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "mdm",
                table: "Catalogs",
                columns: new[] { "CatalogId", "Activo", "CreadoPor", "EmpresaId", "FechaCreacion", "FechaModificacion", "IsDefault", "ModificadoPor", "Name", "Scope" },
                values: new object[] { 1L, true, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, null, "Catálogo General", (byte)1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "mdm",
                table: "Catalogs",
                keyColumn: "CatalogId",
                keyValue: 1L);
        }
    }
}

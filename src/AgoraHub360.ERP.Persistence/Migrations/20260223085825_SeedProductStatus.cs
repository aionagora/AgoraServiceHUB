using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriasProducto",
                schema: "mdm");

            migrationBuilder.DropTable(
                name: "Productos",
                schema: "mdm");

            migrationBuilder.DropTable(
                name: "UnidadesMedida",
                schema: "mdm");

            migrationBuilder.EnsureSchema(
                name: "inv");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "mdm",
                table: "Uoms",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "mdm",
                table: "ProductStatuses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NombreGenerico",
                schema: "mdm",
                table: "Products",
                newName: "GenericName");

            migrationBuilder.RenameColumn(
                name: "NombreComercial",
                schema: "mdm",
                table: "Products",
                newName: "CommercialName");

            migrationBuilder.RenameColumn(
                name: "DescripcionLarga",
                schema: "mdm",
                table: "Products",
                newName: "LongDescription");

            migrationBuilder.RenameColumn(
                name: "DescripcionCorta",
                schema: "mdm",
                table: "Products",
                newName: "ShortDescription");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CatalogId_NombreComercial",
                schema: "mdm",
                table: "Products",
                newName: "IX_Products_CatalogId_CommercialName");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "mdm",
                table: "ProductClassifications",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_ProductClassifications_CatalogId_Type_Nombre",
                schema: "mdm",
                table: "ProductClassifications",
                newName: "IX_ProductClassifications_CatalogId_Type_Name");

            migrationBuilder.RenameColumn(
                name: "Pais",
                schema: "mdm",
                table: "Manufacturers",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "mdm",
                table: "Manufacturers",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Manufacturers_Nombre",
                schema: "mdm",
                table: "Manufacturers",
                newName: "IX_Manufacturers_Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "mdm",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_CatalogId_ParentCategoryId_Nombre",
                schema: "mdm",
                table: "Categories",
                newName: "IX_Categories_CatalogId_ParentCategoryId_Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "mdm",
                table: "Catalogs",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Catalogs_Scope_EmpresaId_Nombre",
                schema: "mdm",
                table: "Catalogs",
                newName: "IX_Catalogs_Scope_EmpresaId_Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "mdm",
                table: "Brands",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Brands_Nombre",
                schema: "mdm",
                table: "Brands",
                newName: "IX_Brands_Name");

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_CompanyProducts_CompanyProductId",
                        column: x => x.CompanyProductId,
                        principalSchema: "mdm",
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockProductos",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockProductos_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockProductos_CompanyProducts_CompanyProductId",
                        column: x => x.CompanyProductId,
                        principalSchema: "mdm",
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "mdm",
                table: "ProductStatuses",
                columns: new[] { "ProductStatusId", "Activo", "Code", "CreadoPor", "FechaCreacion", "FechaModificacion", "IsDefault", "ModificadoPor", "Name" },
                values: new object[,]
                {
                    { 1, true, "ACTIVE", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, null, "Activo" },
                    { 2, true, "INACTIVE", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Inactivo" },
                    { 3, true, "DRAFT", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Borrador" },
                    { 4, true, "DISCONTINUED", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Descontinuado" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_CompanyProductId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "CompanyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_DestinationWarehouseId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId_CompanyProductId_MovementDate",
                schema: "inv",
                table: "MovimientosInventario",
                columns: new[] { "EmpresaId", "CompanyProductId", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId_Number",
                schema: "inv",
                table: "MovimientosInventario",
                columns: new[] { "EmpresaId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId_WarehouseId",
                schema: "inv",
                table: "MovimientosInventario",
                columns: new[] { "EmpresaId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_WarehouseId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_AlmacenId",
                schema: "inv",
                table: "StockProductos",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_CompanyProductId",
                schema: "inv",
                table: "StockProductos",
                column: "CompanyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_EmpresaId",
                schema: "inv",
                table: "StockProductos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_EmpresaId_CompanyProductId_AlmacenId",
                schema: "inv",
                table: "StockProductos",
                columns: new[] { "EmpresaId", "CompanyProductId", "AlmacenId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosInventario",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "StockProductos",
                schema: "inv");

            migrationBuilder.DeleteData(
                schema: "mdm",
                table: "ProductStatuses",
                keyColumn: "ProductStatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "mdm",
                table: "ProductStatuses",
                keyColumn: "ProductStatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "mdm",
                table: "ProductStatuses",
                keyColumn: "ProductStatusId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "mdm",
                table: "ProductStatuses",
                keyColumn: "ProductStatusId",
                keyValue: 4);

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "mdm",
                table: "Uoms",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "mdm",
                table: "ProductStatuses",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                schema: "mdm",
                table: "Products",
                newName: "DescripcionCorta");

            migrationBuilder.RenameColumn(
                name: "LongDescription",
                schema: "mdm",
                table: "Products",
                newName: "DescripcionLarga");

            migrationBuilder.RenameColumn(
                name: "GenericName",
                schema: "mdm",
                table: "Products",
                newName: "NombreGenerico");

            migrationBuilder.RenameColumn(
                name: "CommercialName",
                schema: "mdm",
                table: "Products",
                newName: "NombreComercial");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CatalogId_CommercialName",
                schema: "mdm",
                table: "Products",
                newName: "IX_Products_CatalogId_NombreComercial");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "mdm",
                table: "ProductClassifications",
                newName: "Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_ProductClassifications_CatalogId_Type_Name",
                schema: "mdm",
                table: "ProductClassifications",
                newName: "IX_ProductClassifications_CatalogId_Type_Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "mdm",
                table: "Manufacturers",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "mdm",
                table: "Manufacturers",
                newName: "Pais");

            migrationBuilder.RenameIndex(
                name: "IX_Manufacturers_Name",
                schema: "mdm",
                table: "Manufacturers",
                newName: "IX_Manufacturers_Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "mdm",
                table: "Categories",
                newName: "Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_CatalogId_ParentCategoryId_Name",
                schema: "mdm",
                table: "Categories",
                newName: "IX_Categories_CatalogId_ParentCategoryId_Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "mdm",
                table: "Catalogs",
                newName: "Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_Catalogs_Scope_EmpresaId_Name",
                schema: "mdm",
                table: "Catalogs",
                newName: "IX_Catalogs_Scope_EmpresaId_Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "mdm",
                table: "Brands",
                newName: "Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_Brands_Name",
                schema: "mdm",
                table: "Brands",
                newName: "IX_Brands_Nombre");

            migrationBuilder.CreateTable(
                name: "CategoriasProducto",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasProducto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CategoriaProductoId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ControlStock = table.Column<bool>(type: "bit", nullable: false),
                    CostoBase = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrecioCompra = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TipoProducto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnidadMedidaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Abreviatura = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasProducto_EmpresaId",
                schema: "mdm",
                table: "CategoriasProducto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasProducto_EmpresaId_Nombre",
                schema: "mdm",
                table: "CategoriasProducto",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaProductoId",
                schema: "mdm",
                table: "Productos",
                column: "CategoriaProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId",
                schema: "mdm",
                table: "Productos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId_Codigo",
                schema: "mdm",
                table: "Productos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_UnidadMedidaId",
                schema: "mdm",
                table: "Productos",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_EmpresaId",
                schema: "mdm",
                table: "UnidadesMedida",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_EmpresaId_Abreviatura",
                schema: "mdm",
                table: "UnidadesMedida",
                columns: new[] { "EmpresaId", "Abreviatura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_EmpresaId_Nombre",
                schema: "mdm",
                table: "UnidadesMedida",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);
        }
    }
}

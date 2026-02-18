using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MDM_FullProductManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Schemas ──────────────────────────────────────────────────────
            migrationBuilder.EnsureSchema(name: "rul");
            migrationBuilder.EnsureSchema(name: "ver");
            migrationBuilder.EnsureSchema(name: "prc");
            migrationBuilder.EnsureSchema(name: "cst");
            migrationBuilder.EnsureSchema(name: "doc");

            // ── mdm.ProductStatuses ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductStatuses",
                schema: "mdm",
                columns: table => new
                {
                    ProductStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_ProductStatuses", x => x.ProductStatusId));

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatuses_Code",
                schema: "mdm",
                table: "ProductStatuses",
                column: "Code",
                unique: true);

            // ── mdm.Uoms ─────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Uoms",
                schema: "mdm",
                columns: table => new
                {
                    UomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_Uoms", x => x.UomId));

            migrationBuilder.CreateIndex(
                name: "IX_Uoms_Code",
                schema: "mdm",
                table: "Uoms",
                column: "Code",
                unique: true);

            // ── mdm.Brands ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Brands",
                schema: "mdm",
                columns: table => new
                {
                    BrandId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_Brands", x => x.BrandId));

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Nombre",
                schema: "mdm",
                table: "Brands",
                column: "Nombre",
                unique: true);

            // ── mdm.Manufacturers ────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Manufacturers",
                schema: "mdm",
                columns: table => new
                {
                    ManufacturerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_Manufacturers", x => x.ManufacturerId));

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_Nombre",
                schema: "mdm",
                table: "Manufacturers",
                column: "Nombre");

            // ── mdm.Catalogs ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Catalogs",
                schema: "mdm",
                columns: table => new
                {
                    CatalogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scope = table.Column<byte>(type: "tinyint", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_Catalogs", x => x.CatalogId));

            migrationBuilder.CreateIndex(
                name: "IX_Catalogs_Scope_EmpresaId_Nombre",
                schema: "mdm",
                table: "Catalogs",
                columns: new[] { "Scope", "EmpresaId", "Nombre" },
                unique: true);

            // ── mdm.Products ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Products",
                schema: "mdm",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogId = table.Column<long>(type: "bigint", nullable: false),
                    ProductKind = table.Column<byte>(type: "tinyint", nullable: false),
                    NombreGenerico = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NombreComercial = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    DescripcionCorta = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DescripcionLarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrandId = table.Column<long>(type: "bigint", nullable: true),
                    ManufacturerId = table.Column<long>(type: "bigint", nullable: true),
                    DefaultUomId = table.Column<int>(type: "int", nullable: false),
                    IsStockable = table.Column<bool>(type: "bit", nullable: false),
                    IsSellable = table.Column<bool>(type: "bit", nullable: false),
                    IsPurchasable = table.Column<bool>(type: "bit", nullable: false),
                    LifecycleStatusId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Catalogs",
                        column: x => x.CatalogId,
                        principalSchema: "mdm",
                        principalTable: "Catalogs",
                        principalColumn: "CatalogId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Brands",
                        column: x => x.BrandId,
                        principalSchema: "mdm",
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Products_Manufacturers",
                        column: x => x.ManufacturerId,
                        principalSchema: "mdm",
                        principalTable: "Manufacturers",
                        principalColumn: "ManufacturerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Products_Uoms",
                        column: x => x.DefaultUomId,
                        principalSchema: "mdm",
                        principalTable: "Uoms",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductStatuses",
                        column: x => x.LifecycleStatusId,
                        principalSchema: "mdm",
                        principalTable: "ProductStatuses",
                        principalColumn: "ProductStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatalogId_NombreComercial",
                schema: "mdm",
                table: "Products",
                columns: new[] { "CatalogId", "NombreComercial" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatalogId_BrandId",
                schema: "mdm",
                table: "Products",
                columns: new[] { "CatalogId", "BrandId" });

            // ── mdm.CompanyProducts ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CompanyProducts",
                schema: "mdm",
                columns: table => new
                {
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CodigoInterno = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ImpuestoProfileId = table.Column<int>(type: "int", nullable: true),
                    MonedaBaseId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    IsVisiblePOS = table.Column<bool>(type: "bit", nullable: false),
                    IsVisibleEcommerce = table.Column<bool>(type: "bit", nullable: false),
                    IsVisibleB2B = table.Column<bool>(type: "bit", nullable: false),
                    AllowReturns = table.Column<bool>(type: "bit", nullable: false),
                    WarrantyDays = table.Column<int>(type: "int", nullable: true),
                    MinStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ReorderPoint = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CostingMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProducts", x => x.CompanyProductId);
                    table.ForeignKey(
                        name: "FK_CompanyProducts_Products",
                        column: x => x.ProductId,
                        principalSchema: "mdm",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProducts_EmpresaId",
                schema: "mdm",
                table: "CompanyProducts",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProducts_EmpresaId_Sku",
                schema: "mdm",
                table: "CompanyProducts",
                columns: new[] { "EmpresaId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProducts_EmpresaId_ProductId",
                schema: "mdm",
                table: "CompanyProducts",
                columns: new[] { "EmpresaId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProducts_EmpresaId_Visibility",
                schema: "mdm",
                table: "CompanyProducts",
                columns: new[] { "EmpresaId", "IsVisiblePOS", "IsVisibleEcommerce" });

            // ── mdm.ProductCodes ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductCodes",
                schema: "mdm",
                columns: table => new
                {
                    ProductCodeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    CodeType = table.Column<byte>(type: "tinyint", nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ProviderId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    ChannelId = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCodes", x => x.ProductCodeId);
                    table.ForeignKey(
                        name: "FK_ProductCodes_Products",
                        column: x => x.ProductId,
                        principalSchema: "mdm",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCodes_ProductId_CodeType_IsPrimary",
                schema: "mdm",
                table: "ProductCodes",
                columns: new[] { "ProductId", "CodeType", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCodes_EmpresaId_CodeType_Valor",
                schema: "mdm",
                table: "ProductCodes",
                columns: new[] { "EmpresaId", "CodeType", "Valor" });

            // ── mdm.Categories ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "mdm",
                columns: table => new
                {
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Catalogs",
                        column: x => x.CatalogId,
                        principalSchema: "mdm",
                        principalTable: "Catalogs",
                        principalColumn: "CatalogId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Categories_Parent",
                        column: x => x.ParentCategoryId,
                        principalSchema: "mdm",
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CatalogId_ParentCategoryId_Nombre",
                schema: "mdm",
                table: "Categories",
                columns: new[] { "CatalogId", "ParentCategoryId", "Nombre" },
                unique: true);

            // ── mdm.ProductCategories ────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductCategories",
                schema: "mdm",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => new { x.ProductId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_ProductCategories_Products",
                        column: x => x.ProductId,
                        principalSchema: "mdm",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Categories",
                        column: x => x.CategoryId,
                        principalSchema: "mdm",
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryId",
                schema: "mdm",
                table: "ProductCategories",
                column: "CategoryId");

            // ── mdm.ProductClassifications ───────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductClassifications",
                schema: "mdm",
                columns: table => new
                {
                    ClassificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductClassifications", x => x.ClassificationId);
                    table.ForeignKey(
                        name: "FK_ProductClassifications_Parent",
                        column: x => x.ParentId,
                        principalSchema: "mdm",
                        principalTable: "ProductClassifications",
                        principalColumn: "ClassificationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductClassifications_CatalogId_Type_Nombre",
                schema: "mdm",
                table: "ProductClassifications",
                columns: new[] { "CatalogId", "Type", "Nombre" });

            // ── mdm.ProductClassificationLinks ───────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductClassificationLinks",
                schema: "mdm",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ClassificationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductClassificationLinks", x => new { x.ProductId, x.ClassificationId });
                    table.ForeignKey(
                        name: "FK_ClassLinks_Products",
                        column: x => x.ProductId,
                        principalSchema: "mdm",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassLinks_Classifications",
                        column: x => x.ClassificationId,
                        principalSchema: "mdm",
                        principalTable: "ProductClassifications",
                        principalColumn: "ClassificationId",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── rul.Industries ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Industries",
                schema: "rul",
                columns: table => new
                {
                    IndustryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_Industries", x => x.IndustryId));

            migrationBuilder.CreateIndex(
                name: "IX_Industries_Code",
                schema: "rul",
                table: "Industries",
                column: "Code",
                unique: true);

            // ── mdm.AttributeDefinitions ─────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "AttributeDefinitions",
                schema: "mdm",
                columns: table => new
                {
                    AttributeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DataType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false),
                    IsVariantAxis = table.Column<bool>(type: "bit", nullable: false),
                    ValidationRegex = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    UnitHint = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinitions", x => x.AttributeId);
                    table.ForeignKey(
                        name: "FK_AttributeDefs_Industries",
                        column: x => x.IndustryId,
                        principalSchema: "rul",
                        principalTable: "Industries",
                        principalColumn: "IndustryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_IndustryId_Code",
                schema: "mdm",
                table: "AttributeDefinitions",
                columns: new[] { "IndustryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_IsVariantAxis",
                schema: "mdm",
                table: "AttributeDefinitions",
                column: "IsVariantAxis");

            // ── mdm.AttributeOptions ─────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "AttributeOptions",
                schema: "mdm",
                columns: table => new
                {
                    OptionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeOptions", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_AttrOptions_AttrDefs",
                        column: x => x.AttributeId,
                        principalSchema: "mdm",
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeOptions_AttributeId_Value",
                schema: "mdm",
                table: "AttributeOptions",
                columns: new[] { "AttributeId", "Value" },
                unique: true);

            // ── mdm.ProductAttributes ────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                schema: "mdm",
                columns: table => new
                {
                    ProductAttributeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    AttributeId = table.Column<long>(type: "bigint", nullable: false),
                    ValueString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueDecimal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValueInt = table.Column<int>(type: "int", nullable: true),
                    ValueBool = table.Column<bool>(type: "bit", nullable: true),
                    ValueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ValueJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionId = table.Column<long>(type: "bigint", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.ProductAttributeId);
                    table.ForeignKey(
                        name: "FK_ProductAttrs_Products",
                        column: x => x.ProductId,
                        principalSchema: "mdm",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAttrs_AttrDefs",
                        column: x => x.AttributeId,
                        principalSchema: "mdm",
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductAttrs_Options",
                        column: x => x.OptionId,
                        principalSchema: "mdm",
                        principalTable: "AttributeOptions",
                        principalColumn: "OptionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_ProductId_AttributeId_ValidFrom",
                schema: "mdm",
                table: "ProductAttributes",
                columns: new[] { "ProductId", "AttributeId", "ValidFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_AttributeId",
                schema: "mdm",
                table: "ProductAttributes",
                column: "AttributeId");

            // ── mdm.ProductVariants ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductVariants",
                schema: "mdm",
                columns: table => new
                {
                    VariantId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ParentProductId = table.Column<long>(type: "bigint", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    VariantName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.VariantId);
                    table.ForeignKey(
                        name: "FK_Variants_Products",
                        column: x => x.ParentProductId,
                        principalSchema: "mdm",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_EmpresaId",
                schema: "mdm",
                table: "ProductVariants",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_EmpresaId_Sku",
                schema: "mdm",
                table: "ProductVariants",
                columns: new[] { "EmpresaId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ParentProductId",
                schema: "mdm",
                table: "ProductVariants",
                column: "ParentProductId");

            // ── mdm.VariantAttributeValues ───────────────────────────────────
            migrationBuilder.CreateTable(
                name: "VariantAttributeValues",
                schema: "mdm",
                columns: table => new
                {
                    VariantId = table.Column<long>(type: "bigint", nullable: false),
                    AttributeId = table.Column<long>(type: "bigint", nullable: false),
                    OptionId = table.Column<long>(type: "bigint", nullable: true),
                    ValueString = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantAttributeValues", x => new { x.VariantId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_VarAttrVals_Variants",
                        column: x => x.VariantId,
                        principalSchema: "mdm",
                        principalTable: "ProductVariants",
                        principalColumn: "VariantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VarAttrVals_AttrDefs",
                        column: x => x.AttributeId,
                        principalSchema: "mdm",
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarAttrVals_Options",
                        column: x => x.OptionId,
                        principalSchema: "mdm",
                        principalTable: "AttributeOptions",
                        principalColumn: "OptionId",
                        onDelete: ReferentialAction.SetNull);
                });

            // ── mdm.ProductUoms ──────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductUoms",
                schema: "mdm",
                columns: table => new
                {
                    ProductUomId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    IsBase = table.Column<bool>(type: "bit", nullable: false),
                    FactorToBase = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUoms", x => x.ProductUomId);
                    table.ForeignKey(
                        name: "FK_ProductUoms_Products",
                        column: x => x.ProductId,
                        principalSchema: "mdm",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUoms_Uoms",
                        column: x => x.UomId,
                        principalSchema: "mdm",
                        principalTable: "Uoms",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductUoms_ProductId_UomId",
                schema: "mdm",
                table: "ProductUoms",
                columns: new[] { "ProductId", "UomId" },
                unique: true);

            // ── mdm.CompanyProductFeatures ───────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CompanyProductFeatures",
                schema: "mdm",
                columns: table => new
                {
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: false),
                    FeatureCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProductFeatures", x => new { x.EmpresaId, x.CompanyProductId, x.FeatureCode });
                    table.ForeignKey(
                        name: "FK_Features_CompanyProducts",
                        column: x => x.CompanyProductId,
                        principalSchema: "mdm",
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── rul.ProductIndustryRules ─────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductIndustryRules",
                schema: "rul",
                columns: table => new
                {
                    RuleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    ConditionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductIndustryRules", x => x.RuleId);
                    table.ForeignKey(
                        name: "FK_Rules_Industries",
                        column: x => x.IndustryId,
                        principalSchema: "rul",
                        principalTable: "Industries",
                        principalColumn: "IndustryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductIndustryRules_IndustryId_Priority",
                schema: "rul",
                table: "ProductIndustryRules",
                columns: new[] { "IndustryId", "Priority" });

            // ── ver.EntityVersions ───────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "EntityVersions",
                schema: "ver",
                columns: table => new
                {
                    VersionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<byte>(type: "tinyint", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                    table.PrimaryKey("PK_EntityVersions", x => x.VersionId));

            migrationBuilder.CreateIndex(
                name: "IX_EntityVersions_EntityName_EntityId_VersionNo",
                schema: "ver",
                table: "EntityVersions",
                columns: new[] { "EntityName", "EntityId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityVersions_EntityName_EntityId",
                schema: "ver",
                table: "EntityVersions",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityVersions_EmpresaId",
                schema: "ver",
                table: "EntityVersions",
                column: "EmpresaId");

            // ── prc.PriceLists ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "PriceLists",
                schema: "prc",
                columns: table => new
                {
                    PriceListId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_PriceLists", x => x.PriceListId));

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_EmpresaId",
                schema: "prc",
                table: "PriceLists",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_EmpresaId_Code",
                schema: "prc",
                table: "PriceLists",
                columns: new[] { "EmpresaId", "Code" },
                unique: true);

            // ── prc.PriceListItems ───────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "PriceListItems",
                schema: "prc",
                columns: table => new
                {
                    ItemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceListId = table.Column<long>(type: "bigint", nullable: false),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: true),
                    VariantId = table.Column<long>(type: "bigint", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListItems", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_PriceListItems_PriceLists",
                        column: x => x.PriceListId,
                        principalSchema: "prc",
                        principalTable: "PriceLists",
                        principalColumn: "PriceListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_PriceListId",
                schema: "prc",
                table: "PriceListItems",
                column: "PriceListId");

            // ── cst.CostingRules ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CostingRules",
                schema: "cst",
                columns: table => new
                {
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ProductKind = table.Column<byte>(type: "tinyint", nullable: false),
                    DefaultMethod = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_CostingRules", x => new { x.EmpresaId, x.ProductKind }));

            // ── cst.LandedCostProfiles ───────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "LandedCostProfiles",
                schema: "cst",
                columns: table => new
                {
                    ProfileId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AllocationMethod = table.Column<byte>(type: "tinyint", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_LandedCostProfiles", x => x.ProfileId));

            migrationBuilder.CreateIndex(
                name: "IX_LandedCostProfiles_EmpresaId",
                schema: "cst",
                table: "LandedCostProfiles",
                column: "EmpresaId");

            // ── doc.Documents ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "doc",
                columns: table => new
                {
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    StorageProvider = table.Column<byte>(type: "tinyint", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                    table.PrimaryKey("PK_Documents", x => x.DocumentId));

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EmpresaId",
                schema: "doc",
                table: "Documents",
                column: "EmpresaId");

            // ── doc.ProductDocuments ─────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductDocuments",
                schema: "doc",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    DocType = table.Column<byte>(type: "tinyint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDocuments", x => new { x.ProductId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_ProductDocuments_Documents",
                        column: x => x.DocumentId,
                        principalSchema: "doc",
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductDocuments_ProductId_DocType",
                schema: "doc",
                table: "ProductDocuments",
                columns: new[] { "ProductId", "DocType" });

            // ════════════════════════════════════════════════════════════════
            // PASO 9 — SEEDS MÍNIMOS
            // ════════════════════════════════════════════════════════════════
            var now = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc);

            // ── Estados de producto ──────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "ProductStatuses",
                schema: "mdm",
                columns: new[] { "Code", "Nombre", "IsDefault", "FechaCreacion", "CreadoPor", "Activo" },
                values: new object[,]
                {
                    { "BORRADOR",      "Borrador",      true,  now, "system", true },
                    { "ACTIVO",        "Activo",        false, now, "system", true },
                    { "BLOQUEADO",     "Bloqueado",     false, now, "system", true },
                    { "DESCONTINUADO", "Descontinuado", false, now, "system", true }
                });

            // ── UoM base ─────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Uoms",
                schema: "mdm",
                columns: new[] { "Code", "Nombre", "FechaCreacion", "CreadoPor", "Activo" },
                values: new object[,]
                {
                    { "UND", "Unidad",    now, "system", true },
                    { "CJ",  "Caja",      now, "system", true },
                    { "KG",  "Kilogramo", now, "system", true },
                    { "G",   "Gramo",     now, "system", true },
                    { "L",   "Litro",     now, "system", true },
                    { "ML",  "Mililitro", now, "system", true },
                    { "M",   "Metro",     now, "system", true },
                    { "PAR", "Par",       now, "system", true }
                });

            // ── Industrias ───────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Industries",
                schema: "rul",
                columns: new[] { "Code", "Name", "FechaCreacion", "CreadoPor", "Activo" },
                values: new object[,]
                {
                    { "Farmacia",   "Farmacia y Salud",         now, "system", true },
                    { "Ferreteria", "Ferretería e Industrial",  now, "system", true },
                    { "Extintores", "Extintores y Seguridad",   now, "system", true },
                    { "Alimentos",  "Alimentos y Bebidas",      now, "system", true },
                    { "Produccion", "Manufactura y Producción", now, "system", true },
                    { "General",    "General / Multipropósito", now, "system", true }
                });

            // ── Catálogo global predeterminado ───────────────────────────────
            migrationBuilder.InsertData(
                table: "Catalogs",
                schema: "mdm",
                columns: new[] { "Scope", "EmpresaId", "Nombre", "IsDefault", "FechaCreacion", "CreadoPor", "Activo" },
                values: new object[] { (byte)1, null, "Catálogo Global", true, now, "system", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductDocuments", schema: "doc");
            migrationBuilder.DropTable(name: "Documents", schema: "doc");
            migrationBuilder.DropTable(name: "LandedCostProfiles", schema: "cst");
            migrationBuilder.DropTable(name: "CostingRules", schema: "cst");
            migrationBuilder.DropTable(name: "PriceListItems", schema: "prc");
            migrationBuilder.DropTable(name: "PriceLists", schema: "prc");
            migrationBuilder.DropTable(name: "EntityVersions", schema: "ver");
            migrationBuilder.DropTable(name: "VariantAttributeValues", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductVariants", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductUoms", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductAttributes", schema: "mdm");
            migrationBuilder.DropTable(name: "AttributeOptions", schema: "mdm");
            migrationBuilder.DropTable(name: "AttributeDefinitions", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductIndustryRules", schema: "rul");
            migrationBuilder.DropTable(name: "Industries", schema: "rul");
            migrationBuilder.DropTable(name: "ProductClassificationLinks", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductClassifications", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductCategories", schema: "mdm");
            migrationBuilder.DropTable(name: "Categories", schema: "mdm");
            migrationBuilder.DropTable(name: "CompanyProductFeatures", schema: "mdm");
            migrationBuilder.DropTable(name: "CompanyProducts", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductCodes", schema: "mdm");
            migrationBuilder.DropTable(name: "Products", schema: "mdm");
            migrationBuilder.DropTable(name: "Catalogs", schema: "mdm");
            migrationBuilder.DropTable(name: "Brands", schema: "mdm");
            migrationBuilder.DropTable(name: "Manufacturers", schema: "mdm");
            migrationBuilder.DropTable(name: "ProductStatuses", schema: "mdm");
            migrationBuilder.DropTable(name: "Uoms", schema: "mdm");
        }
    }
}

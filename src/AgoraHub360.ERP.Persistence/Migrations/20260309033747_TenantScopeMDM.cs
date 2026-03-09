using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenantScopeMDM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ══════════════════════════════════════════════════════════════════
            // Step 1a — Add EmpresaId columns to new tables (batch 1)
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF COL_LENGTH('mdm.Products','EmpresaId') IS NULL
                    ALTER TABLE mdm.Products ADD EmpresaId INT NOT NULL CONSTRAINT DF_Products_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.Brands','EmpresaId') IS NULL
                    ALTER TABLE mdm.Brands ADD EmpresaId INT NOT NULL CONSTRAINT DF_Brands_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.Manufacturers','EmpresaId') IS NULL
                    ALTER TABLE mdm.Manufacturers ADD EmpresaId INT NOT NULL CONSTRAINT DF_Manufacturers_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.Categories','EmpresaId') IS NULL
                    ALTER TABLE mdm.Categories ADD EmpresaId INT NOT NULL CONSTRAINT DF_Categories_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.Uoms','EmpresaId') IS NULL
                    ALTER TABLE mdm.Uoms ADD EmpresaId INT NOT NULL CONSTRAINT DF_Uoms_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.ProductStatuses','EmpresaId') IS NULL
                    ALTER TABLE mdm.ProductStatuses ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductStatuses_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.ProductAttributes','EmpresaId') IS NULL
                    ALTER TABLE mdm.ProductAttributes ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductAttributes_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.ProductUoms','EmpresaId') IS NULL
                    ALTER TABLE mdm.ProductUoms ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductUoms_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.ProductClassifications','EmpresaId') IS NULL
                    ALTER TABLE mdm.ProductClassifications ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductClassifications_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.AttributeDefinitions','EmpresaId') IS NULL
                    ALTER TABLE mdm.AttributeDefinitions ADD EmpresaId INT NOT NULL CONSTRAINT DF_AttributeDefinitions_EmpresaId DEFAULT(0);
                IF COL_LENGTH('mdm.AttributeOptions','EmpresaId') IS NULL
                    ALTER TABLE mdm.AttributeOptions ADD EmpresaId INT NOT NULL CONSTRAINT DF_AttributeOptions_EmpresaId DEFAULT(0);
            ");

            // ══════════════════════════════════════════════════════════════════
            // Step 1b — Assign existing data to first empresa (batch 2)
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                DECLARE @EmpId INT = (SELECT TOP 1 Id FROM core.Empresas ORDER BY Id);
                IF @EmpId IS NULL SET @EmpId = 1;

                UPDATE mdm.Products SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.Brands SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.Manufacturers SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.Categories SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.Uoms SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.ProductStatuses SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.ProductAttributes SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.ProductUoms SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.ProductClassifications SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.AttributeDefinitions SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                UPDATE mdm.AttributeOptions SET EmpresaId = @EmpId WHERE EmpresaId = 0;

                -- Catalogs (already has EmpresaId as nullable)
                UPDATE mdm.Catalogs SET EmpresaId = @EmpId WHERE EmpresaId IS NULL OR EmpresaId = 0;
                -- ProductCodes (already has EmpresaId as nullable)
                UPDATE mdm.ProductCodes SET EmpresaId = @EmpId WHERE EmpresaId IS NULL OR EmpresaId = 0;
            ");

            // ══════════════════════════════════════════════════════════════════
            // Step 1c — Drop old indexes that reference EmpresaId (must be before ALTER COLUMN)
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_Code' AND object_id=OBJECT_ID('mdm.Uoms'))
                    DROP INDEX IX_Uoms_Code ON mdm.Uoms;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_Code' AND object_id=OBJECT_ID('mdm.ProductStatuses'))
                    DROP INDEX IX_ProductStatuses_Code ON mdm.ProductStatuses;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_Name' AND object_id=OBJECT_ID('mdm.Manufacturers'))
                    DROP INDEX IX_Manufacturers_Name ON mdm.Manufacturers;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_CatalogId_ParentCategoryId_Name' AND object_id=OBJECT_ID('mdm.Categories'))
                    DROP INDEX IX_Categories_CatalogId_ParentCategoryId_Name ON mdm.Categories;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_Scope_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Catalogs'))
                    DROP INDEX IX_Catalogs_Scope_EmpresaId_Name ON mdm.Catalogs;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_Name' AND object_id=OBJECT_ID('mdm.Brands'))
                    DROP INDEX IX_Brands_Name ON mdm.Brands;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_IndustryId_Code' AND object_id=OBJECT_ID('mdm.AttributeDefinitions'))
                    DROP INDEX IX_AttributeDefinitions_IndustryId_Code ON mdm.AttributeDefinitions;
                -- Also drop ProductCodes index if it uses EmpresaId
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductCodes_EmpresaId_CodeType_Valor' AND object_id=OBJECT_ID('mdm.ProductCodes'))
                    DROP INDEX IX_ProductCodes_EmpresaId_CodeType_Valor ON mdm.ProductCodes;
            ");

            // ══════════════════════════════════════════════════════════════════
            // Step 1d — Alter nullable columns to NOT NULL (batch after index drops)
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"ALTER TABLE mdm.Catalogs ALTER COLUMN EmpresaId INT NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE mdm.ProductCodes ALTER COLUMN EmpresaId INT NOT NULL;");

            // ══════════════════════════════════════════════════════════════════
            // Step 2 — Add new config columns to Empresas
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF COL_LENGTH('core.Empresas','IndustriaId') IS NULL
                    ALTER TABLE core.Empresas ADD IndustriaId TINYINT NOT NULL CONSTRAINT DF_Empresas_IndustriaId DEFAULT(7);
                IF COL_LENGTH('core.Empresas','MetodoCosteoDefault') IS NULL
                    ALTER TABLE core.Empresas ADD MetodoCosteoDefault TINYINT NOT NULL CONSTRAINT DF_Empresas_MetodoCosteoDefault DEFAULT(1);
                IF COL_LENGTH('core.Empresas','PermiteVariantes') IS NULL
                    ALTER TABLE core.Empresas ADD PermiteVariantes BIT NOT NULL CONSTRAINT DF_Empresas_PermiteVariantes DEFAULT(1);
                IF COL_LENGTH('core.Empresas','PermiteLotes') IS NULL
                    ALTER TABLE core.Empresas ADD PermiteLotes BIT NOT NULL CONSTRAINT DF_Empresas_PermiteLotes DEFAULT(0);
                IF COL_LENGTH('core.Empresas','PermiteServicios') IS NULL
                    ALTER TABLE core.Empresas ADD PermiteServicios BIT NOT NULL CONSTRAINT DF_Empresas_PermiteServicios DEFAULT(1);
                IF COL_LENGTH('core.Empresas','AutoGeneraSku') IS NULL
                    ALTER TABLE core.Empresas ADD AutoGeneraSku BIT NOT NULL CONSTRAINT DF_Empresas_AutoGeneraSku DEFAULT(1);
                IF COL_LENGTH('core.Empresas','PrefijoSku') IS NULL
                    ALTER TABLE core.Empresas ADD PrefijoSku NVARCHAR(20) NULL;
            ");

            // ══════════════════════════════════════════════════════════════════
            // Step 3 — Create new tenant-scoped indexes
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                -- Uoms
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_EmpresaId' AND object_id=OBJECT_ID('mdm.Uoms'))
                    CREATE INDEX IX_Uoms_EmpresaId ON mdm.Uoms(EmpresaId);
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_EmpresaId_Code' AND object_id=OBJECT_ID('mdm.Uoms'))
                    CREATE UNIQUE INDEX IX_Uoms_EmpresaId_Code ON mdm.Uoms(EmpresaId, Code);

                -- ProductStatuses
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductStatuses'))
                    CREATE INDEX IX_ProductStatuses_EmpresaId ON mdm.ProductStatuses(EmpresaId);
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_EmpresaId_Code' AND object_id=OBJECT_ID('mdm.ProductStatuses'))
                    CREATE UNIQUE INDEX IX_ProductStatuses_EmpresaId_Code ON mdm.ProductStatuses(EmpresaId, Code);

                -- Products
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Products_EmpresaId' AND object_id=OBJECT_ID('mdm.Products'))
                    CREATE INDEX IX_Products_EmpresaId ON mdm.Products(EmpresaId);

                -- ProductCodes
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductCodes_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductCodes'))
                    CREATE INDEX IX_ProductCodes_EmpresaId ON mdm.ProductCodes(EmpresaId);

                -- ProductClassifications
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductClassifications_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductClassifications'))
                    CREATE INDEX IX_ProductClassifications_EmpresaId ON mdm.ProductClassifications(EmpresaId);

                -- ProductAttributes
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductAttributes_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductAttributes'))
                    CREATE INDEX IX_ProductAttributes_EmpresaId ON mdm.ProductAttributes(EmpresaId);

                -- ProductUoms
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductUoms_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductUoms'))
                    CREATE INDEX IX_ProductUoms_EmpresaId ON mdm.ProductUoms(EmpresaId);

                -- Manufacturers
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_EmpresaId' AND object_id=OBJECT_ID('mdm.Manufacturers'))
                    CREATE INDEX IX_Manufacturers_EmpresaId ON mdm.Manufacturers(EmpresaId);
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Manufacturers'))
                    CREATE UNIQUE INDEX IX_Manufacturers_EmpresaId_Name ON mdm.Manufacturers(EmpresaId, Name);

                -- Categories
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_EmpresaId' AND object_id=OBJECT_ID('mdm.Categories'))
                    CREATE INDEX IX_Categories_EmpresaId ON mdm.Categories(EmpresaId);
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_EmpresaId_CatalogId_ParentCategoryId_Name' AND object_id=OBJECT_ID('mdm.Categories'))
                    CREATE UNIQUE INDEX IX_Categories_EmpresaId_CatalogId_ParentCategoryId_Name ON mdm.Categories(EmpresaId, CatalogId, ParentCategoryId, Name) WHERE ParentCategoryId IS NOT NULL;

                -- Catalogs
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_EmpresaId' AND object_id=OBJECT_ID('mdm.Catalogs'))
                    CREATE INDEX IX_Catalogs_EmpresaId ON mdm.Catalogs(EmpresaId);
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Catalogs'))
                    CREATE UNIQUE INDEX IX_Catalogs_EmpresaId_Name ON mdm.Catalogs(EmpresaId, Name);

                -- Brands
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_EmpresaId' AND object_id=OBJECT_ID('mdm.Brands'))
                    CREATE INDEX IX_Brands_EmpresaId ON mdm.Brands(EmpresaId);
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Brands'))
                    CREATE UNIQUE INDEX IX_Brands_EmpresaId_Name ON mdm.Brands(EmpresaId, Name);

                -- AttributeOptions
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeOptions_EmpresaId' AND object_id=OBJECT_ID('mdm.AttributeOptions'))
                    CREATE INDEX IX_AttributeOptions_EmpresaId ON mdm.AttributeOptions(EmpresaId);

                -- AttributeDefinitions
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_EmpresaId' AND object_id=OBJECT_ID('mdm.AttributeDefinitions'))
                    CREATE INDEX IX_AttributeDefinitions_EmpresaId ON mdm.AttributeDefinitions(EmpresaId);
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_EmpresaId_IndustryId_Code' AND object_id=OBJECT_ID('mdm.AttributeDefinitions'))
                    CREATE UNIQUE INDEX IX_AttributeDefinitions_EmpresaId_IndustryId_Code ON mdm.AttributeDefinitions(EmpresaId, IndustryId, Code);
            ");

            // ══════════════════════════════════════════════════════════════════
            // Step 5 — FK CentrosCosto → Empresas (if missing)
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_CentrosCosto_Empresas_EmpresaId')
                    ALTER TABLE cst.CentrosCosto ADD CONSTRAINT FK_CentrosCosto_Empresas_EmpresaId
                        FOREIGN KEY (EmpresaId) REFERENCES core.Empresas(Id);
            ");

            // ══════════════════════════════════════════════════════════════════
            // Step 6 — Drop temporary default constraints
            // ══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Products_EmpresaId') ALTER TABLE mdm.Products DROP CONSTRAINT DF_Products_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Brands_EmpresaId') ALTER TABLE mdm.Brands DROP CONSTRAINT DF_Brands_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Manufacturers_EmpresaId') ALTER TABLE mdm.Manufacturers DROP CONSTRAINT DF_Manufacturers_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Categories_EmpresaId') ALTER TABLE mdm.Categories DROP CONSTRAINT DF_Categories_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Uoms_EmpresaId') ALTER TABLE mdm.Uoms DROP CONSTRAINT DF_Uoms_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductStatuses_EmpresaId') ALTER TABLE mdm.ProductStatuses DROP CONSTRAINT DF_ProductStatuses_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductAttributes_EmpresaId') ALTER TABLE mdm.ProductAttributes DROP CONSTRAINT DF_ProductAttributes_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductUoms_EmpresaId') ALTER TABLE mdm.ProductUoms DROP CONSTRAINT DF_ProductUoms_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductClassifications_EmpresaId') ALTER TABLE mdm.ProductClassifications DROP CONSTRAINT DF_ProductClassifications_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_AttributeDefinitions_EmpresaId') ALTER TABLE mdm.AttributeDefinitions DROP CONSTRAINT DF_AttributeDefinitions_EmpresaId;
                IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_AttributeOptions_EmpresaId') ALTER TABLE mdm.AttributeOptions DROP CONSTRAINT DF_AttributeOptions_EmpresaId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down is best-effort — remove new columns and indexes, restore old
            migrationBuilder.Sql(@"
                -- Drop new indexes
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_EmpresaId' AND object_id=OBJECT_ID('mdm.Uoms')) DROP INDEX IX_Uoms_EmpresaId ON mdm.Uoms;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_EmpresaId_Code' AND object_id=OBJECT_ID('mdm.Uoms')) DROP INDEX IX_Uoms_EmpresaId_Code ON mdm.Uoms;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductStatuses')) DROP INDEX IX_ProductStatuses_EmpresaId ON mdm.ProductStatuses;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_EmpresaId_Code' AND object_id=OBJECT_ID('mdm.ProductStatuses')) DROP INDEX IX_ProductStatuses_EmpresaId_Code ON mdm.ProductStatuses;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Products_EmpresaId' AND object_id=OBJECT_ID('mdm.Products')) DROP INDEX IX_Products_EmpresaId ON mdm.Products;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductCodes_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductCodes')) DROP INDEX IX_ProductCodes_EmpresaId ON mdm.ProductCodes;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductClassifications_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductClassifications')) DROP INDEX IX_ProductClassifications_EmpresaId ON mdm.ProductClassifications;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductAttributes_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductAttributes')) DROP INDEX IX_ProductAttributes_EmpresaId ON mdm.ProductAttributes;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductUoms_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductUoms')) DROP INDEX IX_ProductUoms_EmpresaId ON mdm.ProductUoms;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_EmpresaId' AND object_id=OBJECT_ID('mdm.Manufacturers')) DROP INDEX IX_Manufacturers_EmpresaId ON mdm.Manufacturers;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Manufacturers')) DROP INDEX IX_Manufacturers_EmpresaId_Name ON mdm.Manufacturers;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_EmpresaId' AND object_id=OBJECT_ID('mdm.Categories')) DROP INDEX IX_Categories_EmpresaId ON mdm.Categories;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_EmpresaId_CatalogId_ParentCategoryId_Name' AND object_id=OBJECT_ID('mdm.Categories')) DROP INDEX IX_Categories_EmpresaId_CatalogId_ParentCategoryId_Name ON mdm.Categories;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_EmpresaId' AND object_id=OBJECT_ID('mdm.Catalogs')) DROP INDEX IX_Catalogs_EmpresaId ON mdm.Catalogs;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Catalogs')) DROP INDEX IX_Catalogs_EmpresaId_Name ON mdm.Catalogs;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_EmpresaId' AND object_id=OBJECT_ID('mdm.Brands')) DROP INDEX IX_Brands_EmpresaId ON mdm.Brands;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Brands')) DROP INDEX IX_Brands_EmpresaId_Name ON mdm.Brands;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeOptions_EmpresaId' AND object_id=OBJECT_ID('mdm.AttributeOptions')) DROP INDEX IX_AttributeOptions_EmpresaId ON mdm.AttributeOptions;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_EmpresaId' AND object_id=OBJECT_ID('mdm.AttributeDefinitions')) DROP INDEX IX_AttributeDefinitions_EmpresaId ON mdm.AttributeDefinitions;
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_EmpresaId_IndustryId_Code' AND object_id=OBJECT_ID('mdm.AttributeDefinitions')) DROP INDEX IX_AttributeDefinitions_EmpresaId_IndustryId_Code ON mdm.AttributeDefinitions;

                -- Drop FK
                IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_CentrosCosto_Empresas_EmpresaId') ALTER TABLE cst.CentrosCosto DROP CONSTRAINT FK_CentrosCosto_Empresas_EmpresaId;

                -- Drop new EmpresaId columns
                IF COL_LENGTH('mdm.Products','EmpresaId') IS NOT NULL ALTER TABLE mdm.Products DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.Brands','EmpresaId') IS NOT NULL ALTER TABLE mdm.Brands DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.Manufacturers','EmpresaId') IS NOT NULL ALTER TABLE mdm.Manufacturers DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.Categories','EmpresaId') IS NOT NULL ALTER TABLE mdm.Categories DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.Uoms','EmpresaId') IS NOT NULL ALTER TABLE mdm.Uoms DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.ProductStatuses','EmpresaId') IS NOT NULL ALTER TABLE mdm.ProductStatuses DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.ProductAttributes','EmpresaId') IS NOT NULL ALTER TABLE mdm.ProductAttributes DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.ProductUoms','EmpresaId') IS NOT NULL ALTER TABLE mdm.ProductUoms DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.ProductClassifications','EmpresaId') IS NOT NULL ALTER TABLE mdm.ProductClassifications DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.AttributeDefinitions','EmpresaId') IS NOT NULL ALTER TABLE mdm.AttributeDefinitions DROP COLUMN EmpresaId;
                IF COL_LENGTH('mdm.AttributeOptions','EmpresaId') IS NOT NULL ALTER TABLE mdm.AttributeOptions DROP COLUMN EmpresaId;

                -- Revert Catalogs.EmpresaId to nullable
                ALTER TABLE mdm.Catalogs ALTER COLUMN EmpresaId INT NULL;

                -- Revert ProductCodes.EmpresaId to nullable
                ALTER TABLE mdm.ProductCodes ALTER COLUMN EmpresaId INT NULL;

                -- Drop Empresa config columns
                IF COL_LENGTH('core.Empresas','IndustriaId') IS NOT NULL ALTER TABLE core.Empresas DROP COLUMN IndustriaId;
                IF COL_LENGTH('core.Empresas','MetodoCosteoDefault') IS NOT NULL ALTER TABLE core.Empresas DROP COLUMN MetodoCosteoDefault;
                IF COL_LENGTH('core.Empresas','PermiteVariantes') IS NOT NULL ALTER TABLE core.Empresas DROP COLUMN PermiteVariantes;
                IF COL_LENGTH('core.Empresas','PermiteLotes') IS NOT NULL ALTER TABLE core.Empresas DROP COLUMN PermiteLotes;
                IF COL_LENGTH('core.Empresas','PermiteServicios') IS NOT NULL ALTER TABLE core.Empresas DROP COLUMN PermiteServicios;
                IF COL_LENGTH('core.Empresas','AutoGeneraSku') IS NOT NULL ALTER TABLE core.Empresas DROP COLUMN AutoGeneraSku;
                IF COL_LENGTH('core.Empresas','PrefijoSku') IS NOT NULL ALTER TABLE core.Empresas DROP COLUMN PrefijoSku;

                -- Restore old indexes
                CREATE UNIQUE INDEX IX_Uoms_Code ON mdm.Uoms(Code);
                CREATE UNIQUE INDEX IX_ProductStatuses_Code ON mdm.ProductStatuses(Code);
                CREATE INDEX IX_Manufacturers_Name ON mdm.Manufacturers(Name);
                CREATE UNIQUE INDEX IX_Categories_CatalogId_ParentCategoryId_Name ON mdm.Categories(CatalogId, ParentCategoryId, Name) WHERE ParentCategoryId IS NOT NULL;
                CREATE UNIQUE INDEX IX_Catalogs_Scope_EmpresaId_Name ON mdm.Catalogs(Scope, EmpresaId, Name) WHERE EmpresaId IS NOT NULL;
                CREATE UNIQUE INDEX IX_Brands_Name ON mdm.Brands(Name);
                CREATE UNIQUE INDEX IX_AttributeDefinitions_IndustryId_Code ON mdm.AttributeDefinitions(IndustryId, Code);
            ");
        }
    }
}

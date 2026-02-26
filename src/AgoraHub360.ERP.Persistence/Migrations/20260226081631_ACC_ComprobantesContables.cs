using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ACC_ComprobantesContables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tipo",
                schema: "acc",
                table: "AsientosContables",
                newName: "TipoRegistro");

            migrationBuilder.AddColumn<string>(
                name: "Concepto",
                schema: "acc",
                table: "AsientosContables",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gestion",
                schema: "acc",
                table: "AsientosContables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NumeroDocumentoPago",
                schema: "acc",
                table: "AsientosContables",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistradoPor",
                schema: "acc",
                table: "AsientosContables",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoCambioId",
                schema: "acc",
                table: "AsientosContables",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoComprobanteId",
                schema: "acc",
                table: "AsientosContables",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPagoId",
                schema: "acc",
                table: "AsientosContables",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTipoCambio",
                schema: "acc",
                table: "AsientosContables",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposCambio",
                schema: "acc",
                columns: table => new
                {
                    TipoCambioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Simbolo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TasaCompra = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TasaVenta = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FechaVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposCambio", x => x.TipoCambioId);
                });

            migrationBuilder.CreateTable(
                name: "TiposComprobante",
                schema: "acc",
                columns: table => new
                {
                    TipoComprobanteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prefijo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposComprobante", x => x.TipoComprobanteId);
                });

            migrationBuilder.CreateTable(
                name: "TiposPago",
                schema: "acc",
                columns: table => new
                {
                    TipoPagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiereReferencia = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPago", x => x.TipoPagoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_Gestion",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "Gestion" });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_TipoCambioId",
                schema: "acc",
                table: "AsientosContables",
                column: "TipoCambioId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_TipoComprobanteId",
                schema: "acc",
                table: "AsientosContables",
                column: "TipoComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_TipoPagoId",
                schema: "acc",
                table: "AsientosContables",
                column: "TipoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposCambio_EmpresaId",
                schema: "acc",
                table: "TiposCambio",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposCambio_EmpresaId_Moneda_FechaVigencia",
                schema: "acc",
                table: "TiposCambio",
                columns: new[] { "EmpresaId", "Moneda", "FechaVigencia" });

            migrationBuilder.CreateIndex(
                name: "IX_TiposComprobante_EmpresaId",
                schema: "acc",
                table: "TiposComprobante",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposComprobante_EmpresaId_Codigo",
                schema: "acc",
                table: "TiposComprobante",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposPago_EmpresaId",
                schema: "acc",
                table: "TiposPago",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposPago_EmpresaId_Codigo",
                schema: "acc",
                table: "TiposPago",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AsientosContables_TiposCambio_TipoCambioId",
                schema: "acc",
                table: "AsientosContables",
                column: "TipoCambioId",
                principalSchema: "acc",
                principalTable: "TiposCambio",
                principalColumn: "TipoCambioId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AsientosContables_TiposComprobante_TipoComprobanteId",
                schema: "acc",
                table: "AsientosContables",
                column: "TipoComprobanteId",
                principalSchema: "acc",
                principalTable: "TiposComprobante",
                principalColumn: "TipoComprobanteId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AsientosContables_TiposPago_TipoPagoId",
                schema: "acc",
                table: "AsientosContables",
                column: "TipoPagoId",
                principalSchema: "acc",
                principalTable: "TiposPago",
                principalColumn: "TipoPagoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsientosContables_TiposCambio_TipoCambioId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropForeignKey(
                name: "FK_AsientosContables_TiposComprobante_TipoComprobanteId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropForeignKey(
                name: "FK_AsientosContables_TiposPago_TipoPagoId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropTable(
                name: "TiposCambio",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "TiposComprobante",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "TiposPago",
                schema: "acc");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_EmpresaId_Gestion",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_TipoCambioId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_TipoComprobanteId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_TipoPagoId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "Concepto",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "Gestion",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "NumeroDocumentoPago",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "RegistradoPor",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "TipoCambioId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "TipoComprobanteId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "TipoPagoId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "ValorTipoCambio",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.RenameColumn(
                name: "TipoRegistro",
                schema: "acc",
                table: "AsientosContables",
                newName: "Tipo");
        }
    }
}

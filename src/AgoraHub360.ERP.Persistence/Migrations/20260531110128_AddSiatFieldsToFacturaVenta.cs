using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiatFieldsToFacturaVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                schema: "vta",
                table: "FacturaVentaDetalles",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroFactura",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAutorizacion",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Leyenda",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailFactura",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuis",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cufd",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuf",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoRecepcion",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoControl",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivityCode",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalDiscount",
                schema: "vta",
                table: "FacturasVenta",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryName",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillUuid",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CodDePago",
                schema: "vta",
                table: "FacturasVenta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescripcionFC",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnlacePdf",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnlaceXml",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiftCardAmount",
                schema: "vta",
                table: "FacturasVenta",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdDosificacion",
                schema: "vta",
                table: "FacturasVenta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTipo",
                schema: "vta",
                table: "FacturasVenta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityDocTypeCode",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origen",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodCode",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PieLey",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Revertido",
                schema: "vta",
                table: "FacturasVenta",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SiatQr",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemCode",
                schema: "vta",
                table: "FacturaVentaDetalles");

            migrationBuilder.DropColumn(
                name: "ActivityCode",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "AdditionalDiscount",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "BeneficiaryName",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "BillUuid",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "CodDePago",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "DescripcionFC",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "EnlacePdf",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "EnlaceXml",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "GiftCardAmount",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "IdDosificacion",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "IdTipo",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "IdentityDocTypeCode",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "Origen",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "PaymentMethodCode",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "PieLey",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "Revertido",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "SiatQr",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroFactura",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAutorizacion",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Leyenda",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 8000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailFactura",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuis",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(800)",
                oldMaxLength: 800,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cufd",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(800)",
                oldMaxLength: 800,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuf",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(800)",
                oldMaxLength: 800,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoRecepcion",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoControl",
                schema: "vta",
                table: "FacturasVenta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}

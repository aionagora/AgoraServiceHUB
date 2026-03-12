using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedOrdenPedidoPlantillasGenericas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Inserta solo si no existe (idempotente).
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [wf].[PlantillasTareas] WHERE [EntityType] = 'OrdenPedido' AND [SubTipo] IS NULL)
                BEGIN
                    INSERT INTO [wf].[PlantillasTareas]
                        ([EntityType],[SubTipo],[Orden],[Codigo],[Descripcion],[EsAutomatico],[RolResponsable],[EmpresaId],[Activo],[FechaCreacion],[CreadoPor],[FechaModificacion],[ModificadoPor])
                    VALUES
                        ('OrdenPedido', NULL, 1, 'OP-CREADA',   'Pedido interno creado en el sistema',          1, NULL,       NULL, 1, '2025-01-01', NULL, NULL, NULL),
                        ('OrdenPedido', NULL, 2, 'REVISION',    'Revisión de disponibilidad de stock',          0, 'ALMACEN',  NULL, 1, '2025-01-01', NULL, NULL, NULL),
                        ('OrdenPedido', NULL, 3, 'APROBACION',  'Aprobación del pedido por el responsable',     0, 'GERENCIA', NULL, 1, '2025-01-01', NULL, NULL, NULL),
                        ('OrdenPedido', NULL, 4, 'OC-GENERADA', 'Orden de compra generada a partir del pedido', 0, 'COMPRAS',  NULL, 1, '2025-01-01', NULL, NULL, NULL),
                        ('OrdenPedido', NULL, 5, 'RECEPCION',   'Mercancía recepcionada en almacén destino',    0, 'ALMACEN',  NULL, 1, '2025-01-01', NULL, NULL, NULL),
                        ('OrdenPedido', NULL, 6, 'CIERRE',      'Pedido cerrado y stock actualizado',           1, NULL,       NULL, 1, '2025-01-01', NULL, NULL, NULL)
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM [wf].[PlantillasTareas]
                WHERE [EntityType] = 'OrdenPedido' AND [SubTipo] IS NULL AND [EmpresaId] IS NULL
            ");
        }
    }
}

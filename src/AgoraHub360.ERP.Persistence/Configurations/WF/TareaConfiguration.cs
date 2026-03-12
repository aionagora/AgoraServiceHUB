namespace AgoraHub360.ERP.Persistence.Configurations.WF;

using AgoraHub360.ERP.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configuración EF Core de la entidad Tarea (schema [wf]).
/// Los índices están diseñados para cubrir el 95 % de los queries de workflow
/// sin necesidad de full-table scans.
/// </summary>
public class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        // ?? Tabla y PK ????????????????????????????????????????????????????????
        builder.ToTable("Tareas", "wf");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityColumn();

        // ?? Propiedades ???????????????????????????????????????????????????????
        builder.Property(t => t.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Descripcion)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("PENDIENTE");

        builder.Property(t => t.Responsable)
            .HasMaxLength(100);

        builder.Property(t => t.Observaciones)
            .HasMaxLength(500);

        builder.Property(t => t.MetadataJson)
            .HasColumnType("nvarchar(max)");

        // Auditoría
        builder.Property(t => t.CreadoPor).HasMaxLength(120);
        builder.Property(t => t.ModificadoPor).HasMaxLength(120);

        // ?? Relaciones ????????????????????????????????????????????????????????
        builder.HasOne(t => t.Empresa)
            .WithMany()
            .HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ?? Índices ???????????????????????????????????????????????????????????

        // PRINCIPAL: recuperar todas las tareas de un documento en orden
        // INCLUDE cubre Estado, Completado, FechaReal sin acceder a la tabla
        builder.HasIndex(t => new { t.EmpresaId, t.EntityType, t.EntityId, t.Orden })
            .HasDatabaseName("IX_Tareas_Entity")
            .HasAnnotation("SqlServer:Include", new[] { "Estado", "Completado", "FechaReal" });

        // Dashboard de tareas pendientes/bloqueadas filtradas por tipo de documento
        builder.HasIndex(t => new { t.EmpresaId, t.Estado, t.EntityType })
            .HasDatabaseName("IX_Tareas_Estado");

        // Unicidad: no puede existir el mismo código en el mismo documento
        builder.HasIndex(t => new { t.EmpresaId, t.EntityType, t.EntityId, t.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Tareas_Codigo_UQ");

        // Reportes de cumplimiento y análisis de retrasos por fecha real
        builder.HasIndex(t => new { t.EmpresaId, t.FechaReal })
            .IncludeProperties(t => new { t.EntityType, t.EntityId })
            .HasDatabaseName("IX_Tareas_Fechas");
    }
}

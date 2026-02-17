namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MonedaConfiguration : IEntityTypeConfiguration<Moneda>
{
    public void Configure(EntityTypeBuilder<Moneda> builder)
    {
        builder.ToTable("Monedas", "core");

        builder.HasKey(m => m.Codigo);

        builder.Property(m => m.Codigo)
            .HasMaxLength(3);

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Simbolo)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(m => m.CreadoPor).HasMaxLength(100);
        builder.Property(m => m.ModificadoPor).HasMaxLength(100);

        // Seed de monedas comunes
        builder.HasData(
            new Moneda { Codigo = "BOB", Nombre = "Boliviano", Simbolo = "Bs", Decimales = 2 },
            new Moneda { Codigo = "USD", Nombre = "Dólar Estadounidense", Simbolo = "$", Decimales = 2 },
            new Moneda { Codigo = "EUR", Nombre = "Euro", Simbolo = "€", Decimales = 2 }
        );
    }
}

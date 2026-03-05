using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Entities.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Configurations;

public sealed class TarifaTemporadaConfiguration : IEntityTypeConfiguration<TarifaTemporada>
{
    public void Configure(EntityTypeBuilder<TarifaTemporada> builder)
    {
        builder.ToTable("TarifaTemporada");
        builder.HasKey(x => x.TarifaTemporadaId);

        builder.Property(x => x.Precio)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasOne<CategoriaHabitacion>()
            .WithMany()
            .HasForeignKey(x => x.CategoriaHabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Temporada>()
            .WithMany()
            .HasForeignKey(x => x.TemporadaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
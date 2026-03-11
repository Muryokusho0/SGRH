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
    public void Configure(EntityTypeBuilder<TarifaTemporada> b)
    {
        b.ToTable("TarifaTemporada");

        b.HasKey(x => x.TarifaTemporadaId);

        b.Property(x => x.TarifaTemporadaId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        b.Property(x => x.CategoriaHabitacionId)
            .IsRequired();

        b.Property(x => x.TemporadaId)
            .IsRequired();

        b.Property(x => x.Precio)
            .HasPrecision(10, 2)
            .IsRequired();

        // CategoriaHabitacion NO expone colección TarifaCategoria → WithMany() sin lambda
        b.HasOne<CategoriaHabitacion>()
            .WithMany()
            .HasForeignKey(p => p.CategoriaHabitacionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Temporada NO expone colección TarifaTemporadas → WithMany() sin lambda
        b.HasOne<Temporada>()
            .WithMany()
            .HasForeignKey(x => x.TemporadaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.TemporadaId, x.CategoriaHabitacionId })
            .HasDatabaseName("IX_TarifaTemporada_Categoria")
            .IsUnique()
            .IncludeProperties(x => x.Precio);
    }
}
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
        builder.Property(x => x.TarifaTemporadaId)
            .ValueGeneratedOnAdd()
            .IsRequired();


        builder.Property(x => x.Precio)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasOne(p => p.CategoriaHabitacion)
            .WithMany(d => d.TarifaCategoria)
            .HasForeignKey(p => p.CategoriaHabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Temporada)
            .WithMany(d => d.TarifaTemporadas)
            .HasForeignKey(x => x.TemporadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TemporadaId, x.CategoriaHabitacionId })
            .HasDatabaseName("IX_TarifaTemporada_Categoria")
            .IsUnique() 
            .IncludeProperties(x => x.Precio); 
    }
}
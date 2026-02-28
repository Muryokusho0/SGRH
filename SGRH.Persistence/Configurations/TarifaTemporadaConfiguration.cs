using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Persistence.Configurations;

public sealed class TarifaTemporadaConfiguration : IEntityTypeConfiguration<TarifaTemporada>
{
    public void Configure(EntityTypeBuilder<TarifaTemporada> b)
    {
        b.ToTable("TarifaTemporada", "dbo");

        b.HasKey(x => x.TarifaTemporadaId);

        b.Property(x => x.TarifaTemporadaId)
            .HasColumnName("TarifaTemporadaId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.CategoriaHabitacionId)
            .HasColumnName("CategoriaHabitacionId")
            .IsRequired();

        b.Property(x => x.TemporadaId)
            .HasColumnName("TemporadaId")
            .IsRequired();

        b.Property(x => x.Precio)
            .HasColumnName("Precio")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        b.HasIndex(x => new { x.TemporadaId, x.CategoriaHabitacionId })
            .IsUnique()
            .HasDatabaseName("IX_TarifaTemporada_Categoria")
            .IncludeProperties(x => x.Precio);

        b.HasOne<CategoriaHabitacion>()
            .WithMany()
            .HasForeignKey(x => x.CategoriaHabitacionId)
            .OnDelete(DeleteBehavior.NoAction);

        b.HasOne<SGRH.Domain.Entities.Temporadas.Temporada>()
            .WithMany()
            .HasForeignKey(x => x.TemporadaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
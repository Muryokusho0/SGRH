using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Persistence.Configurations;

public sealed class HabitacionConfiguration : IEntityTypeConfiguration<Habitacion>
{
    public void Configure(EntityTypeBuilder<Habitacion> b)
    {
        b.ToTable("Habitacion");

        b.HasKey(x => x.HabitacionId);

        b.Property(x => x.HabitacionId)
            .HasColumnName("HabitacionId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.CategoriaHabitacionId)
            .HasColumnName("CategoriaHabitacionId")
            .IsRequired();

        b.Property(x => x.NumeroHabitacion)
            .HasColumnName("NumeroHabitacion")
            .IsRequired();

        b.HasIndex(x => x.NumeroHabitacion).IsUnique();

        // En SQL es DECIMAL(5,0) UNIQUE. En Domain es int. EF lo mapeará a int.
        b.Property(x => x.NumeroHabitacion)
            .HasColumnType("decimal(5,0)");

        b.Property(x => x.Piso)
            .HasColumnName("Piso")
            .IsRequired();

        b.HasIndex(x => x.CategoriaHabitacionId)
            .HasDatabaseName("IX_Habitacion_Categoria")
            .IncludeProperties(x => new { x.NumeroHabitacion, x.Piso });

        b.HasOne(p => p.CategoriaHabitacion)
            .WithMany(d => d.Habitaciones)
            .HasForeignKey(x => x.CategoriaHabitacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

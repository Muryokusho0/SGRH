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
            .HasColumnType("decimal(5,0)")
            .IsRequired();

        b.HasIndex(x => x.NumeroHabitacion)
            .IsUnique();

        b.Property(x => x.Piso)
            .HasColumnName("Piso")
            .IsRequired();

        // EstadoActual es calculado en memoria — no persiste
        b.Ignore(x => x.EstadoActual);

        // Habitacion NO tiene propiedad de navegación CategoriaHabitacion
        b.HasOne<CategoriaHabitacion>()
            .WithMany()
            .HasForeignKey(h => h.CategoriaHabitacionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // _historial → expuesto como IReadOnlyCollection<HabitacionHistorial> Historial
        b.Navigation(h => h.Historial)
            .HasField("_historial")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(x => x.CategoriaHabitacionId)
            .HasDatabaseName("IX_Habitacion_Categoria")
            .IncludeProperties(x => new { x.NumeroHabitacion, x.Piso });
    }
}
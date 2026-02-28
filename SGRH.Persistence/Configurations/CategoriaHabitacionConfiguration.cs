using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Persistence.Configurations;

public sealed class CategoriaHabitacionConfiguration : IEntityTypeConfiguration<CategoriaHabitacion>
{
    public void Configure(EntityTypeBuilder<CategoriaHabitacion> b)
    {
        b.ToTable("CategoriaHabitacion", "dbo");

        b.HasKey(x => x.CategoriaHabitacionId);

        b.Property(x => x.CategoriaHabitacionId)
            .HasColumnName("CategoriaHabitacionId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.NombreCategoria)
            .HasColumnName("nombreCategoria")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        b.HasIndex(x => x.NombreCategoria)
            .IsUnique()
            .HasDatabaseName("IX_CategoriaHabitacion_nombreCategoria");

        b.Property(x => x.Capacidad)
            .HasColumnName("capacidad")
            .IsRequired();

        b.Property(x => x.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(255)
            .IsUnicode(true)
            .IsRequired();

        b.Property(x => x.PrecioBase)
            .HasColumnName("precioBase")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        // Relación 1:N (CategoriaHabitacion -> TarifaTemporada) solo por FK en TarifaTemporada.
        b.Navigation(x => x.Tarifas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

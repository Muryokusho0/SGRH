using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Configurations;

public sealed class ServicioCategoriaPrecioConfiguration
    : IEntityTypeConfiguration<ServicioCategoriaPrecio>
{
    public void Configure(EntityTypeBuilder<ServicioCategoriaPrecio> b)
    {
        b.ToTable("ServicioCategoriaPrecio");

        // PK compuesta
        b.HasKey(x => new { x.ServicioAdicionalId, x.CategoriaHabitacionId });

        b.Property(x => x.Precio)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        // ServicioCategoriaPrecio NO tiene propiedad de navegación ServicioAdicional
        b.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // ServicioCategoriaPrecio NO tiene propiedad de navegación CategoriaHabitacion
        b.HasOne<CategoriaHabitacion>()
            .WithMany()
            .HasForeignKey(x => x.CategoriaHabitacionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.CategoriaHabitacionId, x.ServicioAdicionalId })
            .HasDatabaseName("IX_ServicioCategoriaPrecio_Categoria")
            .IncludeProperties(x => x.Precio);
    }
}
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

public sealed class ServicioCategoriaPrecioConfiguration : IEntityTypeConfiguration<ServicioCategoriaPrecio>
{
    public void Configure(EntityTypeBuilder<ServicioCategoriaPrecio> builder)
    {
        builder.ToTable("ServicioCategoriaPrecio");

        builder.HasKey(x => new { x.ServicioAdicionalId, x.CategoriaHabitacionId });

        builder.Property(x => x.Precio)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CategoriaHabitacion>()
            .WithMany()
            .HasForeignKey(x => x.CategoriaHabitacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
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

        builder.HasOne(p => p.ServicioAdicional)
            .WithMany(d => d.ServicioCategoriaPrecios)
            .HasForeignKey(x => x.ServicioAdicionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CategoriaHabitacion)
            .WithMany(d => d.CategoriaPrecios)
            .HasForeignKey(x => x.CategoriaHabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CategoriaHabitacionId, x.ServicioAdicionalId })
            .HasDatabaseName("IX_ServicioCategoriaPrecio_Categoria")
    .       IncludeProperties(x => new { x.Precio });
    }
}
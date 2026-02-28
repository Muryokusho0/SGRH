using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Servicios;

namespace SGRH.Persistence.Configurations;

public sealed class ServicioCategoriaPrecioConfiguration : IEntityTypeConfiguration<ServicioCategoriaPrecio>
{
    public void Configure(EntityTypeBuilder<ServicioCategoriaPrecio> b)
    {
        b.ToTable("ServicioCategoriaPrecio", "dbo");

        // PK compuesta (ServicioAdicionalId, CategoriaHabitacionId)
        b.HasKey(x => new { x.ServicioAdicionalId, x.CategoriaHabitacionId });

        b.Property(x => x.ServicioAdicionalId)
            .HasColumnName("ServicioAdicionalId")
            .IsRequired();

        b.Property(x => x.CategoriaHabitacionId)
            .HasColumnName("CategoriaHabitacionId")
            .IsRequired();

        b.Property(x => x.Precio)
            .HasColumnName("Precio")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        b.HasIndex(x => new { x.CategoriaHabitacionId, x.ServicioAdicionalId })
            .HasDatabaseName("IX_ServicioCategoriaPrecio_Categoria")
            .IncludeProperties(x => x.Precio);

        b.HasOne<SGRH.Domain.Entities.Servicios.ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .OnDelete(DeleteBehavior.NoAction);

        b.HasOne<SGRH.Domain.Entities.Habitaciones.CategoriaHabitacion>()
            .WithMany()
            .HasForeignKey(x => x.CategoriaHabitacionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
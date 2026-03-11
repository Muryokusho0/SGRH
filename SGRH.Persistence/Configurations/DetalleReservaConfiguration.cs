using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Entities.Reservas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Configurations;

public sealed class DetalleReservaConfiguration : IEntityTypeConfiguration<DetalleReserva>
{
    public void Configure(EntityTypeBuilder<DetalleReserva> b)
    {
        b.ToTable("DetalleReserva");
        b.HasKey(x => x.DetalleReservaId);

        b.Property(x => x.DetalleReservaId)
            .ValueGeneratedOnAdd();

        b.Property(x => x.TarifaAplicada)
            .HasPrecision(10, 2)
            .IsRequired();

        // Reserva → Habitaciones (colección _habitaciones).
        // DetalleReserva NO tiene propiedad de navegación Reserva.
        b.HasOne<Reserva>()
            .WithMany(r => r.Habitaciones)
            .HasForeignKey(d => d.ReservaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Habitacion no expone colección de DetalleReserva.
        b.HasOne<Habitacion>()
            .WithMany()
            .HasForeignKey(d => d.HabitacionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.HabitacionId, x.ReservaId })
            .HasDatabaseName("IX_DetalleReserva_Habitacion")
            .IsUnique()
            .IncludeProperties(x => x.TarifaAplicada);

        b.HasIndex(x => x.ReservaId)
            .HasDatabaseName("IX_DetalleReserva_Reserva")
            .IncludeProperties(x => new { x.HabitacionId, x.TarifaAplicada });
    }
}
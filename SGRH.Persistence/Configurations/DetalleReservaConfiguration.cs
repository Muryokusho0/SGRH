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
    public void Configure(EntityTypeBuilder<DetalleReserva> builder)
    {
        builder.ToTable("DetalleReserva");

        // PK explícita
        builder.HasKey(x => x.DetalleReservaId);

        builder.Property(x => x.DetalleReservaId)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.TarifaAplicada)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.HasOne(p => p.Reserva)
               .WithOne(d => d.DetalleReserva)
               .HasForeignKey<DetalleReserva>(p => p.ReservaId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Habitacion)
               .WithMany(d => d.DetalleReservas)
               .HasForeignKey(p => p.HabitacionId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        // 1. IX_DetalleReserva_Habitacion (UNIQUE)
        builder.HasIndex(x => new { x.HabitacionId, x.ReservaId })
            .HasDatabaseName("IX_DetalleReserva_Habitacion")
            .IsUnique()
            .IncludeProperties(x => x.TarifaAplicada);

        // 2. IX_DetalleReserva_Reserva
        builder.HasIndex(x => x.ReservaId)
            .HasDatabaseName("IX_DetalleReserva_Reserva")
            .IncludeProperties(x => new { x.HabitacionId, x.TarifaAplicada });
    }
}
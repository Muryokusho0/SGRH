using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Reservas;

namespace SGRH.Persistence.Configurations;

public sealed class DetalleReservaConfiguration : IEntityTypeConfiguration<DetalleReserva>
{
    public void Configure(EntityTypeBuilder<DetalleReserva> b)
    {
        b.ToTable("DetalleReserva");
        b.HasKey(x => x.DetalleReservaId);

        b.Property(x => x.TarifaAplicada)   
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        b.HasOne<Reserva>()
            .WithMany()
            .HasForeignKey(x => x.ReservaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<SGRH.Domain.Entities.Habitaciones.Habitacion>()
            .WithMany()
            .HasForeignKey(x => x.HabitacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
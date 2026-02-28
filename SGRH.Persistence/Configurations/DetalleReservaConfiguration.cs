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
        b.ToTable("DetalleReserva", "dbo");

        b.HasKey(x => x.DetalleReservaId);

        b.Property(x => x.DetalleReservaId)
            .HasColumnName("DetalleReservaId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.ReservaId)
            .HasColumnName("ReservaId")
            .IsRequired();

        b.Property(x => x.HabitacionId)
            .HasColumnName("HabitacionId")
            .IsRequired();

        b.Property(x => x.TarifaAplicada)
            .HasColumnName("TarifaAplicada")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        b.HasIndex(x => new { x.HabitacionId, x.ReservaId })
            .IsUnique()
            .HasDatabaseName("IX_DetalleReserva_Habitacion")
            .IncludeProperties(x => x.TarifaAplicada);

        b.HasIndex(x => x.ReservaId)
            .HasDatabaseName("IX_DetalleReserva_Reserva")
            .IncludeProperties(x => new { x.HabitacionId, x.TarifaAplicada });

        b.HasOne<Reserva>()
            .WithMany()
            .HasForeignKey(x => x.ReservaId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<SGRH.Domain.Entities.Habitaciones.Habitacion>()
            .WithMany()
            .HasForeignKey(x => x.HabitacionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
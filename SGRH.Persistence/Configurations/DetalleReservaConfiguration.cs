using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Entities.Reservas;

namespace SGRH.Persistence.Configurations;

public sealed class DetalleReservaConfiguration : IEntityTypeConfiguration<DetalleReserva>
{
    public void Configure(EntityTypeBuilder<DetalleReserva> b)
    {
        b.ToTable("DetalleReserva", t =>
        {
            t.HasTrigger("TR_DetalleReserva_NoPermitirMantenimiento");
            t.HasTrigger("TR_DetalleReserva_NoSolapamientoHabitacion");
            t.HasTrigger("TR_DetalleReserva_CalcularTarifa_Update");
            t.HasTrigger("TR_DetalleReserva_Confirmada_BloquearCambios");
        });

        b.HasKey(x => x.DetalleReservaId);

        b.Property(x => x.DetalleReservaId)
            .ValueGeneratedOnAdd();

        b.Property(x => x.TarifaAplicada)
            .HasPrecision(10, 2)
            .IsRequired();

        b.HasOne<Reserva>()
            .WithMany(r => r.Habitaciones)
            .HasForeignKey(d => d.ReservaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

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
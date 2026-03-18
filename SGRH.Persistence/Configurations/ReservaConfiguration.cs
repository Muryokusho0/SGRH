using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Clientes;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Enums;

namespace SGRH.Persistence.Configurations;

public sealed class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> b)
    {
        b.ToTable("Reserva", t =>
        {
            // EF Core usa OUTPUT para leer IDs generados, pero SQL Server no permite
            // OUTPUT en tablas con triggers. HasTrigger cambia la estrategia a
            // SELECT SCOPE_IDENTITY() que sí es compatible con triggers.
            t.HasTrigger("TR_Reserva_Confirmar_RequiereHabitaciones");
            t.HasTrigger("TR_Reserva_CambioFechas_RevalidarHabitaciones");
            t.HasTrigger("TR_Reserva_Confirmada_BloquearCambioFechas");
        });

        b.HasKey(x => x.ReservaId);

        b.Property(x => x.ReservaId)
            .HasColumnName("ReservaId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.ClienteId)
            .HasColumnName("ClienteId")
            .IsRequired();

        b.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.EstadoReserva)
            .HasColumnName("EstadoReserva")
            .HasConversion(v => v.ToString(), v => Enum.Parse<EstadoReserva>(v))
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.FechaReserva)
            .HasColumnName("FechaReserva")
            .HasColumnType("datetime")
            .IsRequired();

        b.Property(x => x.FechaEntrada)
            .HasColumnName("FechaEntrada")
            .HasColumnType("datetime")
            .IsRequired();

        b.Property(x => x.FechaSalida)
            .HasColumnName("FechaSalida")
            .HasColumnType("datetime")
            .IsRequired();

        b.Ignore(x => x.CostoTotal);

        b.Navigation(x => x.Habitaciones).HasField("_habitaciones");
        b.Navigation(x => x.Servicios).HasField("_servicios");

        b.HasIndex(x => new { x.ClienteId, x.FechaReserva })
            .HasDatabaseName("IX_Reserva_Cliente_Fecha")
            .IsDescending(false, true)
            .IncludeProperties(x => new { x.EstadoReserva, x.FechaEntrada, x.FechaSalida });

        b.HasIndex(x => new { x.FechaEntrada, x.FechaSalida, x.EstadoReserva })
            .HasDatabaseName("IX_Reserva_Rango_Estado")
            .IncludeProperties(x => new { x.ClienteId, x.FechaReserva });
    }
}
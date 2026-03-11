using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Clientes;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Configurations;

public sealed class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> b)
    {
        b.ToTable("Reserva");

        b.HasKey(x => x.ReservaId);

        b.Property(x => x.ReservaId)
            .HasColumnName("ReservaId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.ClienteId)
            .HasColumnName("ClienteId")
            .IsRequired();

        // Cliente NO expone colección de Reservas → WithMany() sin lambda
        b.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // SQL: EstadoReserva VARCHAR(50) CHECK(...)
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

        // Propiedades calculadas — no mapeadas
        b.Ignore(x => x.CostoTotal);

        // Backing fields de las colecciones
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Enums;

namespace SGRH.Persistence.Configurations;

public sealed class HabitacionHistorialConfiguration : IEntityTypeConfiguration<HabitacionHistorial>
{
    public void Configure(EntityTypeBuilder<HabitacionHistorial> b)
    {
        b.ToTable("HabitacionHistorial", "dbo");

        b.HasKey(x => x.HabitacionHistorialId);

        b.Property(x => x.HabitacionHistorialId)
            .HasColumnName("HabitacionHistorialId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.HabitacionId)
            .HasColumnName("HabitacionId")
            .IsRequired();

        // En SQL: VARCHAR(50) con CHECK de valores.
        b.Property(x => x.EstadoHabitacion)
            .HasColumnName("EstadoHabitacion")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<EstadoHabitacion>(v))
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.FechaInicio)
            .HasColumnName("FechaInicio")
            .HasColumnType("datetime")
            .IsRequired();

        b.Property(x => x.FechaFin)
            .HasColumnName("FechaFin")
            .HasColumnType("datetime");

        b.Property(x => x.MotivoCambio)
            .HasColumnName("MotivoCambio")
            .HasMaxLength(255)
            .IsUnicode(true);

        b.HasIndex(x => new { x.HabitacionId, x.FechaInicio })
            .HasDatabaseName("IX_HabitacionHistorial_Habitacion_Fecha")
            .IsDescending(false, true)
            .IncludeProperties(x => new { x.EstadoHabitacion, x.FechaFin, x.MotivoCambio });

        b.HasIndex(x => x.HabitacionId)
            .HasDatabaseName("UX_HabitacionHistorial_Habitacion_Vigente")
            .IsUnique()
            .HasFilter("[FechaFin] IS NULL");

        b.HasOne<Habitacion>()
            .WithMany()
            .HasForeignKey(x => x.HabitacionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

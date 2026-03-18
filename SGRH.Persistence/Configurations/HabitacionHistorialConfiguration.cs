using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Enums;

namespace SGRH.Persistence.Configurations;

public sealed class HabitacionHistorialConfiguration : IEntityTypeConfiguration<HabitacionHistorial>
{
    public void Configure(EntityTypeBuilder<HabitacionHistorial> b)
    {
        b.ToTable("HabitacionHistorial", t =>
        {
            t.HasTrigger("TR_HabitacionHistorial_Consistencia");
        });

        b.HasKey(x => x.HabitacionHistorialId);

        b.Property(x => x.HabitacionHistorialId)
            .ValueGeneratedOnAdd();

        b.Property(x => x.HabitacionId)
            .IsRequired();

        b.Property(x => x.EstadoHabitacion)
            .HasColumnName("EstadoHabitacion")
            .HasConversion(v => v.ToString(), v => Enum.Parse<EstadoHabitacion>(v))
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
            .HasMaxLength(255);

        b.HasOne<Habitacion>()
            .WithMany(h => h.Historial)
            .HasForeignKey(hh => hh.HabitacionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.HabitacionId, x.FechaInicio })
            .HasDatabaseName("IX_HabitacionHistorial_Habitacion_Fecha")
            .IsDescending(false, true)
            .IncludeProperties(x => new { x.EstadoHabitacion, x.FechaFin });
    }
}
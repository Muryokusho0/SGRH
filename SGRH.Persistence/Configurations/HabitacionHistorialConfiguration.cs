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
    public void Configure(EntityTypeBuilder<HabitacionHistorial> builder)
    {
        builder.ToTable("HabitacionHistorial");
        builder.HasKey(x => x.HabitacionHistorialId);

        builder.HasOne(p => p.Habitacion)
            .WithMany(d => d.HabitacionHistorials)
            .HasForeignKey(x => x.HabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.EstadoHabitacion)
            .HasColumnName("EstadoHabitacion")
            .HasConversion(v => v.ToString(), v => Enum.Parse<EstadoHabitacion>(v))
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();
    }
}

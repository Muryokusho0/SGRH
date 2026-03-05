using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Entities.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Configurations;

public sealed class ServicioTemporadaConfiguration : IEntityTypeConfiguration<ServicioTemporada>
{
    public void Configure(EntityTypeBuilder<ServicioTemporada> builder)
    {
        builder.ToTable("ServicioTemporada");
        builder.HasKey(x => new { x.ServicioAdicionalId, x.TemporadaId });

        builder.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Temporada>()
            .WithMany()
            .HasForeignKey(x => x.TemporadaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
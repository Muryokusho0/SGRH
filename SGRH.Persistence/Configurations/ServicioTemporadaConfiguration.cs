using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Servicios;

namespace SGRH.Persistence.Configurations;

public sealed class ServicioTemporadaConfiguration : IEntityTypeConfiguration<ServicioTemporada>
{
    public void Configure(EntityTypeBuilder<ServicioTemporada> b)
    {
        b.ToTable("ServicioTemporada", "dbo");

        b.HasKey(x => new { x.ServicioAdicionalId, x.TemporadaId });

        b.Property(x => x.ServicioAdicionalId)
            .HasColumnName("ServicioAdicionalId")
            .IsRequired();

        b.Property(x => x.TemporadaId)
            .HasColumnName("TemporadaId")
            .IsRequired();

        b.HasIndex(x => new { x.TemporadaId, x.ServicioAdicionalId })
            .HasDatabaseName("IX_ServicioTemporada_Temporada");

        b.HasOne<SGRH.Domain.Entities.Servicios.ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .OnDelete(DeleteBehavior.NoAction);

        b.HasOne<SGRH.Domain.Entities.Temporadas.Temporada>()
            .WithMany()
            .HasForeignKey(x => x.TemporadaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
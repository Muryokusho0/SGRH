using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Entities.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Persistence.Context;

namespace SGRH.Persistence.Configurations;

/// <summary>
/// <summary>
/// Entidad de persistencia pura — gestiona la tabla junction ServicioTemporada.
/// La carga de TemporadaIds en ServicioAdicional se realiza manualmente en el repositorio.
/// </summary>
public sealed class ServicioTemporadaConfiguration : IEntityTypeConfiguration<ServicioTemporada>
{
    public void Configure(EntityTypeBuilder<ServicioTemporada> b)
    {
        b.ToTable("ServicioTemporada");

        // PK compuesta
        b.HasKey(x => new { x.ServicioAdicionalId, x.TemporadaId });

        // ServicioTemporada NO tiene nav prop ServicioAdicional
        // ServicioAdicional NO expone colección de ServicioTemporada
        b.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // ServicioTemporada NO tiene nav prop Temporada
        // Temporada NO expone colección de ServicioTemporada
        b.HasOne<Temporada>()
            .WithMany()
            .HasForeignKey(x => x.TemporadaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.TemporadaId, x.ServicioAdicionalId })
            .HasDatabaseName("IX_ServicioTemporada_Temporada");
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Temporadas;

namespace SGRH.Persistence.Configurations;

public sealed class TemporadaConfiguration : IEntityTypeConfiguration<Temporada>
{
    public void Configure(EntityTypeBuilder<Temporada> b)
    {
        b.ToTable("Temporada", "dbo");

        b.HasKey(x => x.TemporadaId);

        b.Property(x => x.TemporadaId)
            .HasColumnName("TemporadaId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.NombreTemporada)
            .HasColumnName("nombreTemporada")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.FechaInicio)
            .HasColumnName("fechaInicio")
            .HasColumnType("date")
            .IsRequired();

        b.Property(x => x.FechaFin)
            .HasColumnName("fechaFin")
            .HasColumnType("date")
            .IsRequired();

        b.HasIndex(x => new { x.FechaInicio, x.FechaFin })
            .HasDatabaseName("IX_Temporada_Rango")
            .IncludeProperties(x => x.NombreTemporada);
    }
}
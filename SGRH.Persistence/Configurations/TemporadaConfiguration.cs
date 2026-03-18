using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Temporadas;

namespace SGRH.Persistence.Configurations;

public sealed class TemporadaConfiguration : IEntityTypeConfiguration<Temporada>
{
    public void Configure(EntityTypeBuilder<Temporada> b)
    {
        b.ToTable("Temporada", "dbo", t =>
        {
            t.HasTrigger("TR_Temporada_NoSolapamiento");
        });

        b.HasKey(x => x.TemporadaId);

        b.Property(x => x.TemporadaId)
            .HasColumnName("TemporadaId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.NombreTemporada)
            .HasColumnName("nombreTemporada")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        // ── Modo específico ──────────────────────────────────────
        b.Property(x => x.FechaInicio)
            .HasColumnName("fechaInicio")
            .HasColumnType("date")
            .IsRequired(false);

        b.Property(x => x.FechaFin)
            .HasColumnName("fechaFin")
            .HasColumnType("date")
            .IsRequired(false);

        // ── Modo recurrente ──────────────────────────────────────
        b.Property(x => x.EsRecurrente)
            .HasColumnName("EsRecurrente")
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(x => x.MesInicio)
            .HasColumnName("MesInicio")
            .IsRequired(false);

        b.Property(x => x.DiaInicio)
            .HasColumnName("DiaInicio")
            .IsRequired(false);

        b.Property(x => x.MesFin)
            .HasColumnName("MesFin")
            .IsRequired(false);

        b.Property(x => x.DiaFin)
            .HasColumnName("DiaFin")
            .IsRequired(false);

        b.HasIndex(x => new { x.FechaInicio, x.FechaFin })
            .HasDatabaseName("IX_Temporada_Rango")
            .IncludeProperties(x => x.NombreTemporada);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Entities.Temporadas;

namespace SGRH.Persistence.Configurations;

public sealed class ServicioTemporadaConfiguration : IEntityTypeConfiguration<ServicioTemporada>
{
    public void Configure(EntityTypeBuilder<ServicioTemporada> b)
    {
        b.ToTable("ServicioTemporada");

        b.HasKey(x => new { x.ServicioAdicionalId, x.TemporadaId });

        b.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<Temporada>()
            .WithMany()
            .HasForeignKey(x => x.TemporadaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.TemporadaId, x.ServicioAdicionalId })
            .HasDatabaseName("IX_ServicioTemporada_Temporada");
    }
}
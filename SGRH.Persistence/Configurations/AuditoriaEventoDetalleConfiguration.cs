using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Auditoria;

namespace SGRH.Persistence.Configurations;

public sealed class AuditoriaEventoDetalleConfiguration : IEntityTypeConfiguration<AuditoriaEventoDetalle>
{
    public void Configure(EntityTypeBuilder<AuditoriaEventoDetalle> b)
    {
        b.ToTable("AuditoriaEventoDetalle");
        b.HasKey(x => x.AuditoriaEventoDetalleId);
        b.Property(x => x.AuditoriaEventoDetalleId)
            .ValueGeneratedOnAdd();

        b.HasOne(p => p.AuditoriaEvento)
            .WithOne(d => d.AuditoriaEventoDetalle)
            .HasForeignKey<AuditoriaEventoDetalle>(p => p.AuditoriaEventoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.Campo)
            .HasMaxLength(100);

        b.HasIndex(x => x.AuditoriaEventoId)
            .HasDatabaseName("IX_AuditoriaEventoDetalle_Evento")
            .IncludeProperties(x => x.Campo);
    }
}

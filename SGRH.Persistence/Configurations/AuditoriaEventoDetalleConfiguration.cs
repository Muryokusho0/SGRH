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

        b.Property(x => x.AuditoriaEventoId)
            .IsRequired();

        b.Property(x => x.Campo)
            .HasColumnName("Campo")
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.ValorAnterior)
            .HasColumnName("ValorAnterior");

        b.Property(x => x.ValorNuevo)
            .HasColumnName("ValorNuevo");

        // AuditoriaEvento tiene prop pública Detalles (backing field _detalles).
        // AuditoriaEventoDetalle NO tiene propiedad de navegación de regreso.
        b.HasOne<AuditoriaEvento>()
            .WithMany(a => a.Detalles)
            .HasForeignKey(d => d.AuditoriaEventoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.AuditoriaEventoId)
            .HasDatabaseName("IX_AuditoriaEventoDetalle_Evento")
            .IncludeProperties(x => x.Campo);
    }
}
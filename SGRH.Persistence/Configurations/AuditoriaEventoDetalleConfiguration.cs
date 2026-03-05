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
    public void Configure(EntityTypeBuilder<AuditoriaEventoDetalle> builder)
    {
        builder.ToTable("AuditoriaEventoDetalle");
        builder.HasKey(x => x.AuditoriaEventoDetalleId);

        builder.HasOne<AuditoriaEvento>()
            .WithMany()
            .HasForeignKey(x => x.AuditoriaEventoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

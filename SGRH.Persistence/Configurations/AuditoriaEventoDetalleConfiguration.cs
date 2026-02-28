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
        b.ToTable("AuditoriaEventoDetalle", "dbo");

        b.HasKey(x => x.AuditoriaEventoDetalleId);

        b.Property(x => x.AuditoriaEventoDetalleId)
            .HasColumnName("AuditoriaEventoDetalleId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.AuditoriaEventoId)
            .HasColumnName("AuditoriaEventoId")
            .IsRequired();

        b.Property(x => x.Campo)
            .HasColumnName("Campo")
            .HasMaxLength(128)
            .IsUnicode(true)
            .IsRequired();

        b.Property(x => x.ValorAnterior)
            .HasColumnName("ValorAnterior")
            .HasColumnType("nvarchar(max)");

        b.Property(x => x.ValorNuevo)
            .HasColumnName("ValorNuevo")
            .HasColumnType("nvarchar(max)");

        b.HasOne<AuditoriaEvento>()
            .WithMany()
            .HasForeignKey(x => x.AuditoriaEventoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

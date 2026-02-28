using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Auditoria;

namespace SGRH.Persistence.Configurations;

public sealed class AuditoriaEventoConfiguration : IEntityTypeConfiguration<AuditoriaEvento>
{
    public void Configure(EntityTypeBuilder<AuditoriaEvento> b)
    {
        b.ToTable("AuditoriaEvento", "dbo");

        b.HasKey(x => x.AuditoriaEventoId);

        b.Property(x => x.AuditoriaEventoId)
            .HasColumnName("AuditoriaEventoId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.FechaUtc)
            .HasColumnName("FechaUtc")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        b.Property(x => x.UsuarioId)
            .HasColumnName("UsuarioId");

        b.Property(x => x.Rol)
            .HasColumnName("Rol")
            .HasMaxLength(20)
            .IsUnicode(false);

        b.Property(x => x.UsernameSnapshot)
            .HasColumnName("UsernameSnapshot")
            .HasMaxLength(100)
            .IsUnicode(true);

        b.Property(x => x.Accion)
            .HasColumnName("Accion")
            .HasMaxLength(50)
            .IsUnicode(true)
            .IsRequired();

        b.Property(x => x.Modulo)
            .HasColumnName("Modulo")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired();

        b.Property(x => x.Entidad)
            .HasColumnName("Entidad")
            .HasMaxLength(100)
            .IsUnicode(true);

        b.Property(x => x.EntidadId)
            .HasColumnName("EntidadId")
            .HasMaxLength(64)
            .IsUnicode(true);

        b.Property(x => x.RequestId)
            .HasColumnName("RequestId");

        b.Property(x => x.IpOrigen)
            .HasColumnName("IpOrigen")
            .HasMaxLength(45)
            .IsUnicode(false);

        b.Property(x => x.UserAgent)
            .HasColumnName("UserAgent")
            .HasMaxLength(255)
            .IsUnicode(true);

        b.Property(x => x.Descripcion)
            .HasColumnName("Descripcion")
            .HasMaxLength(500)
            .IsUnicode(true);

        b.Navigation(x => x.Detalles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
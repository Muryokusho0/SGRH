using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Seguridad;

namespace SGRH.Persistence.Configurations;

public sealed class AuditoriaEventoConfiguration : IEntityTypeConfiguration<AuditoriaEvento>
{
    public void Configure(EntityTypeBuilder<AuditoriaEvento> b)
    {
        b.ToTable("AuditoriaEvento");
        b.HasKey(x => x.AuditoriaEventoId);

        b.Property(x => x.AuditoriaEventoId)
            .ValueGeneratedOnAdd();

        b.Property(x => x.FechaUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired();

        // Usuario NO expone colección Auditorias → WithMany() sin lambda
        b.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(d => d.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.Rol)
            .HasColumnName("Rol")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.UsernameSnapshot)
            .HasColumnName("UsernameSnapshot")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.Accion)
            .HasColumnName("Accion")
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.Modulo)
            .HasColumnName("Modulo")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.Entidad)
            .HasColumnName("Entidad")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.EntidadId)
            .HasColumnName("EntidadId")
            .HasMaxLength(64)
            .IsRequired();

        b.Property(x => x.RequestId)
            .HasColumnName("RequestId")
            .IsRequired();

        b.Property(x => x.IpOrigen)
            .HasColumnName("IpOrigen")
            .HasMaxLength(45)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.UserAgent)
            .HasColumnName("UserAgent")
            .HasMaxLength(255)
            .IsRequired();

        b.Property(x => x.Descripcion)
            .HasColumnName("Descripcion")
            .HasMaxLength(500)
            .IsRequired();

        b.Navigation(x => x.Detalles)
            .HasField("_detalles");

        b.HasIndex(x => x.FechaUtc)
            .HasDatabaseName("IX_AuditoriaEvento_FechaUtc")
            .IsDescending()
            .IncludeProperties(x => new { x.UsuarioId, x.Rol, x.Accion, x.Modulo, x.Entidad, x.EntidadId });

        b.HasIndex(x => new { x.UsuarioId, x.FechaUtc })
            .HasDatabaseName("IX_AuditoriaEvento_Usuario_FechaUtc")
            .IsDescending(false, true)
            .IncludeProperties(x => new { x.Accion, x.Modulo, x.Entidad, x.EntidadId });

        b.HasIndex(x => new { x.Modulo, x.FechaUtc })
            .HasDatabaseName("IX_AuditoriaEvento_Modulo_FechaUtc")
            .IsDescending(false, true)
            .IncludeProperties(x => new { x.Accion, x.UsuarioId, x.Entidad, x.EntidadId });
    }
}
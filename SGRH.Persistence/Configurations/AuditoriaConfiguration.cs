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

        b.HasOne(d => d.Usuario)
            .WithMany(p => p.Auditorias)
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.Rol)
            .HasColumnName("Rol")
            .HasMaxLength(20)
            .IsUnicode(false);

        b.Property(x => x.UsernameSnapshot)
            .HasColumnName("UsernameSnapshot")
            .HasMaxLength(100);

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
            .HasMaxLength(100);

        b.Property(x => x.EntidadId)
            .HasColumnName("EntidadId")
            .HasMaxLength(64);

        b.Property(x => x.RequestId)
            .HasColumnName("RequestId");

        b.Property(x => x.IpOrigen)
            .HasColumnName("IpOrigen")
            .HasMaxLength(45)
            .IsUnicode(false);

        b.Property(x => x.UserAgent)
            .HasColumnName("UserAgent")
            .HasMaxLength(255);

        b.Property(x => x.Descripcion)
            .HasColumnName("Descripcion")
            .HasMaxLength(500);

        // 1. IX_AuditoriaEvento_FechaUtc
        b.HasIndex(x => x.FechaUtc)
            .HasDatabaseName("IX_AuditoriaEvento_FechaUtc")
            .IsDescending() // Orden descendente para la fecha
            .IncludeProperties(x => new {
                x.UsuarioId,
                x.Rol,
                x.Accion,
                x.Modulo,
                x.Entidad,
                x.EntidadId
            });

        // 2. IX_AuditoriaEvento_Usuario_FechaUtc
        b.HasIndex(x => new { x.UsuarioId, x.FechaUtc })
            .HasDatabaseName("IX_AuditoriaEvento_Usuario_FechaUtc")
            // UsuarioId es Ascendente (false), FechaUtc es Descendente (true)
            .IsDescending(false, true)
            .IncludeProperties(x => new {
                x.Accion,
                x.Modulo,
                x.Entidad,
                x.EntidadId
            });

        // 3. IX_AuditoriaEvento_Modulo_FechaUtc
        b.HasIndex(x => new { x.Modulo, x.FechaUtc })
            .HasDatabaseName("IX_AuditoriaEvento_Modulo_FechaUtc")
            // Modulo es Ascendente (false), FechaUtc es Descendente (true)
            .IsDescending(false, true)
            .IncludeProperties(x => new {
                x.Accion,
                x.UsuarioId,
                x.Entidad,
                x.EntidadId
            });
    }
}
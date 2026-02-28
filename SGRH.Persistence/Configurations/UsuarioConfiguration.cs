using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;

namespace SGRH.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuario", "dbo");

        b.HasKey(x => x.UsuarioId);

        b.Property(x => x.UsuarioId)
            .HasColumnName("UsuarioId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.ClienteId)
            .HasColumnName("ClienteId");

        b.Property(x => x.Username)
            .HasColumnName("Username")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired();

        b.HasIndex(x => x.Username)
            .IsUnique()
            .HasDatabaseName("UQ_Usuario_Username");

        b.Property(x => x.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasMaxLength(255)
            .IsUnicode(true)
            .IsRequired();

        // SQL: Rol VARCHAR(20) con CHECK. Domain: enum RolUsuario.
        b.Property(x => x.Rol)
            .HasColumnName("Rol")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<RolUsuario>(v))
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.Activo)
            .HasColumnName("Activo")
            .IsRequired();

        // AuditableEntity (CreatedAtUtc) => en SQL CreatedAt DATETIME2(3)
        b.Property(x => x.CreatedAtUtc)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        b.HasIndex(x => x.ClienteId)
            .HasDatabaseName("IX_Usuario_Cliente")
            .IncludeProperties(x => new { x.Rol, x.Activo, x.Username });

        b.HasIndex(x => new { x.Rol, x.Activo })
            .HasDatabaseName("IX_Usuario_Rol_Activo")
            .IncludeProperties(x => new { x.Username, x.ClienteId });

        b.HasOne<SGRH.Domain.Entities.Clientes.Cliente>()
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
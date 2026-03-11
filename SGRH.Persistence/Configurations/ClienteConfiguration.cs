using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Clientes;

namespace SGRH.Persistence.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Cliente");

        b.HasKey(x => x.ClienteId);

        b.Property(x => x.ClienteId)
            .ValueGeneratedOnAdd();

        b.Property(x => x.NationalId)
            .HasColumnName("NationalID")
            .HasMaxLength(20)
            .IsUnicode(false);

        b.HasIndex(x => x.NationalId).IsUnique();

        b.Property(x => x.NombreCliente)
            .HasColumnName("nombreCliente")
            .HasMaxLength(100);

        b.Property(x => x.ApellidoCliente)
            .HasColumnName("apellidoCliente")
            .HasMaxLength(100);

        b.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        b.HasIndex(x => x.Email).IsUnique();

        b.Property(x => x.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();
    }
}
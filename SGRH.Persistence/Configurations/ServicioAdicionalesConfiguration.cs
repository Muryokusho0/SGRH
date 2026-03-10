using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Servicios;

namespace SGRH.Persistence.Configurations;

public sealed class ServicioAdicionalConfiguration : IEntityTypeConfiguration<ServicioAdicional>
{
    public void Configure(EntityTypeBuilder<ServicioAdicional> b)
    {
        b.ToTable("ServicioAdicional");

        b.HasKey(x => x.ServicioAdicionalId);

        b.Property(x => x.ServicioAdicionalId)
            .HasColumnName("ServicioAdicionalId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.NombreServicio)
            .HasColumnName("nombreServicio")
            .HasMaxLength(50)
            .IsUnicode(true)
            .IsRequired();

        b.Property(x => x.TipoServicio)
            .HasColumnName("tipoServicio")
            .HasMaxLength(50)
            .IsUnicode(true)
            .IsRequired();

        b.HasIndex(x => new { x.TipoServicio, x.NombreServicio })
            .HasDatabaseName("IX_ServicioAdicional_Tipo_Nombre");
    }
}
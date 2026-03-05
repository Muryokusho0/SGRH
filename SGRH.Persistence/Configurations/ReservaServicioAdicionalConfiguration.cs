using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Entities.Servicios;

namespace SGRH.Persistence.Configurations;

public sealed class ReservaServicioAdicionalConfiguration : IEntityTypeConfiguration<ReservaServicioAdicional>
{
    public void Configure(EntityTypeBuilder<ReservaServicioAdicional> builder)
    {
        builder.ToTable("ReservaServicioAdicional");
        builder.HasKey(x => x.ReservaServicioAdicionalId);

        builder.Property(x => x.Cantidad).IsRequired();

        builder.Property(x => x.PrecioUnitarioAplicado)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Ignore(x => x.SubTotalAplicado);

        builder.HasOne<Reserva>()
            .WithMany()
            .HasForeignKey(x => x.ReservaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
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

public sealed class ReservaServicioAdicionalConfiguration
    : IEntityTypeConfiguration<ReservaServicioAdicional>
{
    public void Configure(EntityTypeBuilder<ReservaServicioAdicional> b)
    {
        b.ToTable("ReservaServicioAdicional");
        b.HasKey(x => x.ReservaServicioAdicionalId);

        b.Property(x => x.ReservaServicioAdicionalId)
            .ValueGeneratedOnAdd();

        b.Property(x => x.Cantidad)
            .IsRequired();

        b.Property(x => x.PrecioUnitarioAplicado)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        // SubTotal es calculado en memoria — no persiste
        b.Ignore(x => x.SubTotal);

        // RSA NO tiene propiedad de navegación Reserva.
        // La colección inversa _servicios se expone como Servicios en Reserva.
        b.HasOne<Reserva>()
            .WithMany(r => r.Servicios)
            .HasForeignKey(x => x.ReservaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // RSA NO tiene propiedad de navegación ServicioAdicional.
        b.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
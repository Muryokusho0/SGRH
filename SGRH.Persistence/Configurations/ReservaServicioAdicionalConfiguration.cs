using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Entities.Reservas;

namespace SGRH.Persistence.Configurations;

public sealed class ReservaServicioAdicionalConfiguration : IEntityTypeConfiguration<ReservaServicioAdicional>
{
    public void Configure(EntityTypeBuilder<ReservaServicioAdicional> b)
    {
        b.ToTable("ReservaServicioAdicional", "dbo");

        b.HasKey(x => x.ReservaServicioAdicionalId);

        b.Property(x => x.ReservaServicioAdicionalId)
            .HasColumnName("ReservaServicioAdicionalId")
            .ValueGeneratedOnAdd();

        b.Property(x => x.ReservaId)
            .HasColumnName("ReservaId")
            .IsRequired();

        b.Property(x => x.ServicioAdicionalId)
            .HasColumnName("ServicioAdicionalId")
            .IsRequired();

        b.Property(x => x.Cantidad)
            .HasColumnName("Cantidad")
            .IsRequired();

        b.Property(x => x.PrecioUnitarioAplicado)
            .HasColumnName("PrecioUnitarioAplicado")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        // SubTotalAplicado es computed persisted en SQL.
        // En Domain es una propiedad calculada => no mapear a columna.
        b.Ignore(x => x.SubTotalAplicado);

        b.HasIndex(x => x.ReservaId)
            .HasDatabaseName("IX_RSA_Reserva")
            .IncludeProperties(x => new { x.ServicioAdicionalId, x.Cantidad, x.PrecioUnitarioAplicado });

        b.HasIndex(x => x.ServicioAdicionalId)
            .HasDatabaseName("IX_RSA_Servicio")
            .IncludeProperties(x => new { x.ReservaId, x.Cantidad, x.PrecioUnitarioAplicado });

        b.HasIndex(x => new { x.ReservaId, x.ServicioAdicionalId })
            .IsUnique()
            .HasDatabaseName("uq_reserva_servicio");

        b.HasOne<SGRH.Domain.Entities.Reservas.Reserva>()
            .WithMany()
            .HasForeignKey(x => x.ReservaId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<SGRH.Domain.Entities.Servicios.ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
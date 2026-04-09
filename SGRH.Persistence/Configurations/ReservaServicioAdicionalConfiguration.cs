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
        b.ToTable("ReservaServicioAdicional", t =>
        {
            t.HasTrigger("TR_RSA_Confirmada_BloquearCambios");
            t.HasTrigger("TR_RSA_CalcularPrecio_Update");
        });

        b.HasKey(x => x.ReservaServicioAdicionalId);

        b.Property(x => x.ReservaServicioAdicionalId)
            .ValueGeneratedOnAdd();

        b.Property(x => x.Cantidad)
            .IsRequired();

        b.Property(x => x.PrecioUnitarioAplicado)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        // SubTotal es calculado en memoria — no persiste desde EF.
        // SubTotalAplicado es columna computada PERSISTED en SQL Server.
        b.Ignore(x => x.SubTotal);

        b.HasOne<Reserva>()
            .WithMany(r => r.Servicios)
            .HasForeignKey(x => x.ReservaId)
            .IsRequired()
            // ClientCascade: EF Core elimina el ReservaServicioAdicional huérfano
            // cuando se remueve de Reserva.Servicios en memoria.
            // Mismo razonamiento que DetalleReservaConfiguration.
            .OnDelete(DeleteBehavior.ClientCascade);

        // La relación ServicioAdicional → RSA sigue siendo Restrict.
        b.HasOne<ServicioAdicional>()
            .WithMany()
            .HasForeignKey(x => x.ServicioAdicionalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
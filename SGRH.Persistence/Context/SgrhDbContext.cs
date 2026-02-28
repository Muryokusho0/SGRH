using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Entities.Clientes;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Entities.Temporadas;
using SGRH.Domain.Entities.Auditoria;

namespace SGRH.Persistence.Context;

public class SGRHDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public SGRHDbContext(DbContextOptions<SGRHDbContext> options)
          : base(options)
    {
    }

    // CLIENTES
    public DbSet<Cliente> Clientes => Set<Cliente>();

    // HABITACIONES
    public DbSet<CategoriaHabitacion> CategoriasHabitacion => Set<CategoriaHabitacion>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<HabitacionHistorial> HabitacionHistorial => Set<HabitacionHistorial>();
    public DbSet<TarifaTemporada> TarifasTemporada => Set<TarifaTemporada>();

    // RESERVAS
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<DetalleReserva> DetallesReserva => Set<DetalleReserva>();
    public DbSet<ReservaServicioAdicional> ReservaServiciosAdicionales => Set<ReservaServicioAdicional>();

    // SERVICIOS
    public DbSet<ServicioAdicional> ServiciosAdicionales => Set<ServicioAdicional>();
    public DbSet<ServicioCategoriaPrecio> ServicioCategoriaPrecios => Set<ServicioCategoriaPrecio>();
    public DbSet<ServicioTemporada> ServicioTemporadas => Set<ServicioTemporada>();

    // TEMPORADAS
    public DbSet<Temporada> Temporadas => Set<Temporada>();

    // SEGURIDAD
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    // AUDITORIA
    public DbSet<AuditoriaEvento> AuditoriaEventos => Set<AuditoriaEvento>();
    public DbSet<AuditoriaEventoDetalle> AuditoriaEventoDetalles => Set<AuditoriaEventoDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SGRHDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

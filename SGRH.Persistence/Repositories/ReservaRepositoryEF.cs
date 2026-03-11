using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Entities.Reservas;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Enums;

namespace SGRH.Persistence.Repositories;

public sealed class ReservaRepositoryEF
    : Repository<Reserva, int>, IReservaRepository
{
    public ReservaRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<Reserva?> GetByIdWithDetallesAsync(
        int reservaId, CancellationToken ct = default)
        => Db.Reservas
            .Include(r => r.Habitaciones)
            .Include(r => r.Servicios)
            .FirstOrDefaultAsync(r => r.ReservaId == reservaId, ct);

    public Task<List<Reserva>> GetByClienteAsync(
        int clienteId, CancellationToken ct = default)
        => Db.Reservas
            .AsNoTracking()
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.FechaReserva)
            .ToListAsync(ct);

    public Task<bool> HabitacionTieneReservaActivaAsync(
        int habitacionId,
        DateTime entrada,
        DateTime salida,
        int? excluirReservaId,
        CancellationToken ct = default)
        => (from dr in Db.DetallesReserva.AsNoTracking()
            join r in Db.Reservas.AsNoTracking() on dr.ReservaId equals r.ReservaId
            where dr.HabitacionId == habitacionId
               && (excluirReservaId == null || dr.ReservaId != excluirReservaId)
               && r.EstadoReserva != EstadoReserva.Cancelada
               && r.FechaEntrada < salida
               && r.FechaSalida > entrada
            select dr.DetalleReservaId)
           .AnyAsync(ct);
}
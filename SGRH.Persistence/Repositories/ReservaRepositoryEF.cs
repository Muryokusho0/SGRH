using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Enums;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

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

    public Task<List<Reserva>> BuscarAsync(
        int? clienteId,
        string? estado,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        DateTime? reservadaDesde,
        DateTime? reservadaHasta,
        CancellationToken ct = default)
    {
        var query = Db.Reservas.AsNoTracking();

        if (clienteId.HasValue)
            query = query.Where(r => r.ClienteId == clienteId.Value);

        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoReserva>(estado, out var estadoEnum))
            query = query.Where(r => r.EstadoReserva == estadoEnum);

        if (fechaDesde.HasValue)
            query = query.Where(r => r.FechaEntrada >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(r => r.FechaEntrada <= fechaHasta.Value);

        if (reservadaDesde.HasValue)
            query = query.Where(r => r.FechaReserva >= reservadaDesde.Value);

        if (reservadaHasta.HasValue)
            query = query.Where(r => r.FechaReserva <= reservadaHasta.Value);

        return query
            .OrderByDescending(r => r.FechaReserva)
            .ToListAsync(ct);
    }
}
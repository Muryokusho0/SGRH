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

namespace SGRH.Persistence.Repositories;

public sealed class ReservaRepositoryEF : Repository<Reserva, int>, IReservaRepository
{
    public ReservaRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<Reserva?> GetByIdWithDetallesAsync(int reservaId, CancellationToken ct = default)
        => Db.Reservas
            .Include(r => r.Habitaciones)
            .Include(r => r.Servicios)
            .FirstOrDefaultAsync(r => r.ReservaId == reservaId, ct);

    public Task<List<Reserva>> GetByClienteAsync(int clienteId, CancellationToken ct = default)
        => Db.Reservas
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.FechaReserva)
            .ToListAsync(ct);
}
using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Reservas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IReservaRepository : IRepository<Reserva, int>
{
    Task<Reserva?> GetByIdWithDetallesAsync(int reservaId, CancellationToken ct = default);

    Task<List<Reserva>> GetByClienteAsync(int clienteId, CancellationToken ct = default);
}

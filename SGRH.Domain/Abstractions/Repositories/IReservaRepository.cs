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
    Task<Reserva?> GetByIdWithDetallesAsync(
        int reservaId, CancellationToken ct = default);

    Task<List<Reserva>> GetByClienteAsync(
        int clienteId, CancellationToken ct = default);

    Task<bool> HabitacionTieneReservaActivaAsync(
        int habitacionId,
        DateTime entrada,
        DateTime salida,
        int? excluirReservaId,
        CancellationToken ct = default);

    Task<List<Reserva>> BuscarAsync(
        int? clienteId,
        string? estado,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        DateTime? reservadaDesde,
        DateTime? reservadaHasta,
        CancellationToken ct = default);
}


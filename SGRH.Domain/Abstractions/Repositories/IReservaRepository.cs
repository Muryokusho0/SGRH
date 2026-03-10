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
    // Carga la reserva con DetalleReserva y ReservaServicioAdicional.
    // Es el método principal — casi todos los casos de uso lo necesitan.
    Task<Reserva?> GetByIdWithDetallesAsync(
        int reservaId, CancellationToken ct = default);

    // Historial de reservas de un cliente.
    Task<List<Reserva>> GetByClienteAsync(
        int clienteId, CancellationToken ct = default);

    // Verifica si una habitación tiene reserva activa en el rango dado.
    // Lo usa la implementación de IReservaDomainPolicy.
    Task<bool> HabitacionTieneReservaActivaAsync(
        int habitacionId,
        DateTime entrada,
        DateTime salida,
        int? excluirReservaId,
        CancellationToken ct = default);
}

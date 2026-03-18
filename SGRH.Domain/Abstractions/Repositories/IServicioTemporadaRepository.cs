using SGRH.Domain.Entities.Servicios;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IServicioTemporadaRepository
{
    Task<bool> ExisteAsync(int servicioId, int temporadaId, CancellationToken ct = default);
    Task AddAsync(ServicioTemporada entity, CancellationToken ct = default);
}
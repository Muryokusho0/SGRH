using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

public interface ITemporadaRepository : IRepository<Temporada, int>
{
    Task<Temporada?> GetByFechaAsync(DateTime fecha, CancellationToken ct = default);

    Task<bool> ExisteSolapamientoAsync(
        DateTime fechaInicio, DateTime fechaFin, int? excludeId,
        CancellationToken ct = default);

    Task<List<Temporada>> BuscarAsync(
        string? nombre, DateTime? fechaDesde, DateTime? fechaHasta,
        CancellationToken ct = default);
}

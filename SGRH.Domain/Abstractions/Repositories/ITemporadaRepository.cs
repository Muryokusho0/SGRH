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
}
using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IServicioCategoriaPrecioRepository
    : IRepository<ServicioCategoriaPrecio, (int ServicioId, int CategoriaId)>
{
    Task<decimal?> GetPrecioAsync(
        int servicioId, int categoriaId, CancellationToken ct = default);
}
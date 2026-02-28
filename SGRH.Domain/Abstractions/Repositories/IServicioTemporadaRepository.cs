using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IServicioTemporadaRepository : IRepository<ServicioTemporada, (int ServicioId, int TemporadaId)>
{
}

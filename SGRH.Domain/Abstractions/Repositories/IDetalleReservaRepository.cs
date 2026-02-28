using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Reservas;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IDetalleReservaRepository : IRepository<DetalleReserva, int>
{
}

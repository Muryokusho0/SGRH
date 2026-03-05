using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Reservas;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class DetalleReservaRepositoryEF
    : Repository<DetalleReserva, int>, IDetalleReservaRepository
{
    public DetalleReservaRepositoryEF(SGRHDbContext db) : base(db) { }
}

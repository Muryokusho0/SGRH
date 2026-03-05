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

public sealed class ReservaServicioAdicionalRepositoryEF
    : Repository<ReservaServicioAdicional, int>, IReservaServicioAdicionalRepository
{
    public ReservaServicioAdicionalRepositoryEF(SGRHDbContext db) : base(db) { }
}
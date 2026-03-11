using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Abstractions.Services.Time;

namespace SGRH.Infrastructure.Services;

// Implementación concreta de ISystemClock.
// Vive en Infrastructure, no en Domain, para mantener
// el dominio libre de implementaciones concretas.
public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}






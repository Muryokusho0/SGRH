using SGRH.Domain.Abstractions.Services.Time;
using SGRH.Domain.Common;

namespace SGRH.Infrastructure.Services;

// Implementación concreta de ISystemClock.
// Devuelve la hora local en UTC-4 (República Dominicana).
// Vive en Infrastructure, no en Domain, para mantener
// el dominio libre de implementaciones concretas.
public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => HoraLocal.Ahora;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services.Time;

/// <summary>
/// Define el contrato para obtener la fecha y hora actual del sistema.
/// Permite reemplazar el reloj del sistema por una implementación controlada
/// en pruebas unitarias (patrón de inyección de dependencias para el tiempo).
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Obtiene la fecha y hora actual en UTC.
    /// </summary>
    DateTime UtcNow { get; }
}

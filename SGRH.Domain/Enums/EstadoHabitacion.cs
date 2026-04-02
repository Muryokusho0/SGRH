using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Enums;

/// <summary>
/// Representa el estado operativo actual de una habitación dentro del hotel.
/// El historial de cambios de estado se registra en <c>HabitacionHistorial</c>.
/// </summary>
public enum EstadoHabitacion
{
    /// <summary>
    /// La habitación está libre y puede ser reservada o asignada.
    /// </summary>
    Disponible,

    /// <summary>
    /// La habitación está actualmente ocupada por un huésped con reserva activa.
    /// </summary>
    Ocupada,

    /// <summary>
    /// La habitación está fuera de servicio por trabajos de mantenimiento.
    /// Requiere un motivo de cambio al registrar el estado.
    /// </summary>
    Mantenimiento,

    /// <summary>
    /// La habitación está siendo acondicionada por el personal de limpieza.
    /// Requiere un motivo de cambio al registrar el estado.
    /// </summary>
    Limpieza
}

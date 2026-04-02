using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Enums;

/// <summary>
/// Representa el ciclo de vida de una reserva dentro del sistema.
/// Las transiciones de estado están controladas por los métodos de la entidad <c>Reserva</c>.
/// </summary>
public enum EstadoReserva
{
    /// <summary>
    /// La reserva fue creada pero aún no ha sido confirmada por el recepcionista o el sistema.
    /// En este estado puede modificarse (fechas, habitaciones, servicios).
    /// </summary>
    Pendiente,

    /// <summary>
    /// La reserva ha sido confirmada y no puede ser editada.
    /// Solo puede cancelarse o finalizarse.
    /// </summary>
    Confirmada,

    /// <summary>
    /// La reserva fue cancelada antes de su finalización.
    /// No puede modificarse ni reactivarse.
    /// </summary>
    Cancelada,

    /// <summary>
    /// La reserva fue completada con éxito (el huésped realizó su estancia).
    /// Es un estado terminal; no puede modificarse ni cancelarse.
    /// </summary>
    Finalizada
}
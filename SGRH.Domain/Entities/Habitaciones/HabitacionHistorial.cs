using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Habitaciones;

/// <summary>
/// Registra un período de tiempo en que una habitación tuvo un estado particular.
/// Implementa un historial bitemporal: el registro está vigente si <see cref="FechaFin"/> es <c>null</c>.
/// </summary>
public sealed class HabitacionHistorial : EntityBase
{
    /// <summary>
    /// Identificador único del registro de historial.
    /// </summary>
    public int HabitacionHistorialId { get; private set; }

    /// <summary>
    /// Identificador de la habitación a la que pertenece este registro de historial.
    /// </summary>
    public int HabitacionId { get; private set; }

    /// <summary>
    /// Estado que tenía la habitación durante el período registrado.
    /// </summary>
    public EstadoHabitacion EstadoHabitacion { get; private set; }

    /// <summary>
    /// Fecha y hora (hora local UTC-4) en que comenzó este estado.
    /// </summary>
    public DateTime FechaInicio { get; private set; }

    /// <summary>
    /// Fecha y hora en que terminó este estado. Si es <c>null</c>, el registro sigue vigente.
    /// </summary>
    public DateTime? FechaFin { get; private set; }

    /// <summary>
    /// Motivo del cambio de estado. Obligatorio para estados
    /// <see cref="EstadoHabitacion.Mantenimiento"/> y <see cref="EstadoHabitacion.Limpieza"/>.
    /// </summary>
    public string? MotivoCambio { get; private set; }

    private HabitacionHistorial() { }

    /// <summary>
    /// Crea un nuevo registro de historial para una habitación con el estado indicado.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación afectada.</param>
    /// <param name="estado">Nuevo estado de la habitación.</param>
    /// <param name="motivo">
    /// Motivo del cambio. Requerido para <see cref="EstadoHabitacion.Mantenimiento"/>
    /// y <see cref="EstadoHabitacion.Limpieza"/>; debe ser <c>null</c> en otros estados.
    /// </param>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si el motivo no cumple con los requisitos del estado indicado.
    /// </exception>
    public HabitacionHistorial(int habitacionId, EstadoHabitacion estado, string? motivo)
    {
        // habitacionId puede ser 0 cuando la Habitacion padre aún no fue guardada.
        // EF Core asigna el FK automáticamente al hacer SaveChanges.

        if (estado is EstadoHabitacion.Mantenimiento or EstadoHabitacion.Limpieza)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ValidationException(
                    $"El estado {estado} requiere un motivo de cambio.");
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(motivo))
                throw new ValidationException(
                    $"El estado {estado} no debe tener motivo de cambio.");
        }

        if (motivo is not null)
            Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo), 255);

        HabitacionId = habitacionId;
        EstadoHabitacion = estado;
        MotivoCambio = motivo;

        // ← Hora local UTC-4, no UTC
        FechaInicio = HoraLocal.Ahora;
        // FechaFin queda null → este registro es el vigente
    }

    /// <summary>
    /// Cierra el registro de historial vigente asignando la fecha y hora actuales a <see cref="FechaFin"/>.
    /// </summary>
    /// <exception cref="Exceptions.BusinessRuleViolationException">
    /// Se lanza si el registro ya fue cerrado previamente (es decir, <see cref="FechaFin"/> no es <c>null</c>).
    /// </exception>
    public void Cerrar()
    {
        if (FechaFin is not null)
            throw new BusinessRuleViolationException(
                "Este registro de historial ya está cerrado.");

        // ← Hora local UTC-4, no UTC
        FechaFin = HoraLocal.Ahora;
    }

    protected override object GetKey() => HabitacionHistorialId;
}
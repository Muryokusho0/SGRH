using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Habitaciones;

public sealed class HabitacionHistorial : EntityBase
{
    public int HabitacionHistorialId { get; private set; }
    public int HabitacionId { get; private set; }
    public EstadoHabitacion EstadoHabitacion { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime? FechaFin { get; private set; }
    public string? MotivoCambio { get; private set; }

    private HabitacionHistorial() { }

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
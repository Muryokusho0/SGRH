using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Enums;
using SGRH.Domain.Common;
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
        Guard.AgainstOutOfRange(habitacionId, nameof(habitacionId), 0);

        // Replicar el CHECK constraint de la BD:
        // Mantenimiento y Limpieza REQUIEREN motivo.
        // Disponible y Ocupada NO deben tener motivo.
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
        FechaInicio = DateTime.UtcNow;
        MotivoCambio = motivo;
        // FechaFin queda null → este registro es el vigente
    }

    public void Cerrar()
    {
        if (FechaFin is not null)
            throw new BusinessRuleViolationException(
                "Este registro de historial ya está cerrado.");

        FechaFin = DateTime.UtcNow;
    }

    protected override object GetKey() => HabitacionHistorialId;
}
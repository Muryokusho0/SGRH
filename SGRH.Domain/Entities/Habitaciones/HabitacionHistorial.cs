using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Enums;

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
        HabitacionId = habitacionId;
        EstadoHabitacion = estado;
        FechaInicio = DateTime.UtcNow;
        MotivoCambio = motivo;
    }

    public void CerrarEstado()
    {
        FechaFin = DateTime.UtcNow;
    }

    protected override object GetKey() => HabitacionHistorialId;
}
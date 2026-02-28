using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;

namespace SGRH.Domain.Entities.Habitaciones;
public sealed class Habitacion : EntityBase
{
    public int HabitacionId { get; private set; }
    public int CategoriaHabitacionId { get; private set; }
    public int NumeroHabitacion { get; private set; }
    public int Piso { get; private set; }

    private readonly List<HabitacionHistorial> _historial = new();
    public IReadOnlyCollection<HabitacionHistorial> Historial => _historial;

    private Habitacion() { }

    public Habitacion(int categoriaHabitacionId, int numeroHabitacion, int piso)
    {
        if (numeroHabitacion <= 0)
            throw new DomainException("Número de habitación inválido.");

        if (piso <= 0)
            throw new DomainException("Piso inválido.");

        CategoriaHabitacionId = categoriaHabitacionId;
        NumeroHabitacion = numeroHabitacion;
        Piso = piso;
    }

    public void CambiarEstado(EstadoHabitacion estado, string? motivo = null)
    {
        var nuevo = new HabitacionHistorial(HabitacionId, estado, motivo);
        _historial.Add(nuevo);
    }

    protected override object GetKey() => HabitacionId;
}

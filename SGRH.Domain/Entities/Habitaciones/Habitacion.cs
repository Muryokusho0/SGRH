using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Habitaciones;

public sealed class Habitacion : EntityBase
{
    public int HabitacionId { get; private set; }
    public int CategoriaHabitacionId { get; private set; }
    public int NumeroHabitacion { get; private set; }
    public int Piso { get; private set; }

    // Colección privada — nadie puede modificar el historial directamente
    // desde fuera. Solo a través del método CambiarEstado().
    private readonly List<HabitacionHistorial> _historial = [];
    public IReadOnlyCollection<HabitacionHistorial> Historial => _historial;

    // Propiedad calculada: devuelve el registro vigente (FechaFin == null).
    // Útil para saber el estado actual sin consultar la BD.
    public HabitacionHistorial? EstadoActual
        => _historial.FirstOrDefault(h => h.FechaFin is null);

    private Habitacion() { }

    public Habitacion(int categoriaHabitacionId, int numeroHabitacion, int piso)
    {
        Guard.AgainstOutOfRange(categoriaHabitacionId, nameof(categoriaHabitacionId), 0);
        Guard.AgainstOutOfRange(numeroHabitacion, nameof(numeroHabitacion), 0);
        Guard.AgainstOutOfRange(piso, nameof(piso), 0);

        CategoriaHabitacionId = categoriaHabitacionId;
        NumeroHabitacion = numeroHabitacion;
        Piso = piso;

        _historial.Add(new HabitacionHistorial(HabitacionId, EstadoHabitacion.Disponible, null));
    }

    public void CambiarEstado(EstadoHabitacion nuevoEstado, string? motivo = null)
    {
        // Verificar que no sea el mismo estado actual
        if (EstadoActual?.EstadoHabitacion == nuevoEstado)
            throw new BusinessRuleViolationException(
                $"La habitación ya se encuentra en estado {nuevoEstado}.");

        // Cerrar el registro vigente actual
        EstadoActual?.Cerrar();

        // Crear el nuevo registro vigente
        // HabitacionHistorial valida internamente las reglas de motivo
        _historial.Add(new HabitacionHistorial(HabitacionId, nuevoEstado, motivo));
    }

    public void CambiarCategoria(int nuevaCategoriaId)
    {
        Guard.AgainstOutOfRange(nuevaCategoriaId, nameof(nuevaCategoriaId), 0);
        CategoriaHabitacionId = nuevaCategoriaId;
    }

    protected override object GetKey() => HabitacionId;
}

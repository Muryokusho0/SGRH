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

    private readonly List<HabitacionHistorial> _historial = [];
    public IReadOnlyCollection<HabitacionHistorial> Historial => _historial;

    // Propiedad calculada: devuelve el registro vigente (FechaFin == null).
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

        // El registro inicial de HabitacionHistorial (estado Disponible) lo crea
        // automáticamente el trigger TR_HabitacionHistorial_Consistencia en la BD.
        // No lo creamos aquí para evitar duplicados.
    }

    public void CambiarEstado(EstadoHabitacion nuevoEstado, string? motivo = null)
    {
        if (EstadoActual?.EstadoHabitacion == nuevoEstado)
            throw new BusinessRuleViolationException(
                $"La habitación ya se encuentra en estado {nuevoEstado}.");

        // Cerrar el registro vigente actual
        EstadoActual?.Cerrar();

        // Agregar el nuevo registro vigente
        _historial.Add(new HabitacionHistorial(HabitacionId, nuevoEstado, motivo));
    }

    public void CambiarCategoria(int nuevaCategoriaId)
    {
        Guard.AgainstOutOfRange(nuevaCategoriaId, nameof(nuevaCategoriaId), 0);
        CategoriaHabitacionId = nuevaCategoriaId;
    }

    protected override object GetKey() => HabitacionId;
}
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Habitaciones;

/// <summary>
/// Representa una habitación física del hotel. Mantiene su estado actual
/// y el historial completo de cambios de estado mediante <see cref="HabitacionHistorial"/>.
/// </summary>
public sealed class Habitacion : EntityBase
{
    /// <summary>
    /// Identificador único de la habitación.
    /// </summary>
    public int HabitacionId { get; private set; }

    /// <summary>
    /// Identificador de la categoría a la que pertenece esta habitación.
    /// </summary>
    public int CategoriaHabitacionId { get; private set; }

    /// <summary>
    /// Número visible de la habitación que se muestra a los huéspedes y recepcionistas.
    /// </summary>
    public int NumeroHabitacion { get; private set; }

    /// <summary>
    /// Número de piso donde se encuentra la habitación.
    /// </summary>
    public int Piso { get; private set; }

    private readonly List<HabitacionHistorial> _historial = [];

    /// <summary>
    /// Historial completo de cambios de estado de la habitación.
    /// </summary>
    public IReadOnlyCollection<HabitacionHistorial> Historial => _historial;

    /// <summary>
    /// Estado operativo actual de la habitación: el registro de historial con <c>FechaFin == null</c>.
    /// Puede ser <c>null</c> si el historial no ha sido cargado desde la base de datos.
    /// </summary>
    public HabitacionHistorial? EstadoActual
        => _historial.FirstOrDefault(h => h.FechaFin is null);

    private Habitacion() { }

    /// <summary>
    /// Crea una nueva habitación asignada a una categoría, con número y piso específicos.
    /// </summary>
    /// <remarks>
    /// El estado inicial <see cref="EstadoHabitacion.Disponible"/> es creado automáticamente
    /// por el trigger <c>TR_HabitacionHistorial_Consistencia</c> en la base de datos.
    /// </remarks>
    /// <param name="categoriaHabitacionId">Id de la categoría de habitación (mayor a 0).</param>
    /// <param name="numeroHabitacion">Número visible de la habitación (mayor a 0).</param>
    /// <param name="piso">Número de piso (mayor a 0).</param>
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

    /// <summary>
    /// Cambia el estado operativo de la habitación, cerrando el registro vigente
    /// y creando uno nuevo con el estado indicado.
    /// </summary>
    /// <param name="nuevoEstado">Nuevo estado que tendrá la habitación.</param>
    /// <param name="motivo">
    /// Motivo del cambio. Requerido si el nuevo estado es
    /// <see cref="EstadoHabitacion.Mantenimiento"/> o <see cref="EstadoHabitacion.Limpieza"/>.
    /// </param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">
    /// Se lanza si la habitación ya se encuentra en el estado indicado.
    /// </exception>
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

    /// <summary>
    /// Cambia la categoría a la que pertenece esta habitación.
    /// </summary>
    /// <param name="nuevaCategoriaId">Id de la nueva categoría de habitación (mayor a 0).</param>
    public void CambiarCategoria(int nuevaCategoriaId)
    {
        Guard.AgainstOutOfRange(nuevaCategoriaId, nameof(nuevaCategoriaId), 0);
        CategoriaHabitacionId = nuevaCategoriaId;
    }

    protected override object GetKey() => HabitacionId;
}
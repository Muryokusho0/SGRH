using SGRH.Web.Models.Reservas;

namespace SGRH.Web.Services.Reservas;

/// <summary>
/// Contrato del servicio de reservas para el portal del cliente.
/// Todos los métodos lanzan <see cref="InvalidOperationException"/>
/// con mensajes ya traducidos y listos para mostrarse en la UI.
/// </summary>
public interface IReservaService
{
    /// <summary>Lista las reservas del cliente autenticado.</summary>
    Task<List<ReservaViewModel>> ListarMisReservasAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene el detalle completo de una reserva.
    /// Devuelve <c>null</c> si no existe.
    /// </summary>
    Task<DetalleReservaViewModel?> ObtenerDetalleAsync(int reservaId, CancellationToken ct = default);

    /// <summary>
    /// Crea una reserva base con las fechas indicadas.
    /// Devuelve el ID interno de la reserva creada (solo para uso del servicio).
    /// </summary>
    Task<int> CrearAsync(DateTime fechaEntrada, DateTime fechaSalida, CancellationToken ct = default);

    /// <summary>Confirma una reserva pendiente.</summary>
    Task ConfirmarAsync(int reservaId, CancellationToken ct = default);

    /// <summary>Cancela una reserva.</summary>
    Task CancelarAsync(int reservaId, CancellationToken ct = default);

    /// <summary>Cambia las fechas de una reserva pendiente.</summary>
    Task CambiarFechasAsync(
        int reservaId, DateTime nuevaEntrada, DateTime nuevaSalida,
        CancellationToken ct = default);

    /// <summary>
    /// Agrega una habitación a una reserva por su número visible.
    /// El cliente selecciona la habitación desde una tarjeta; este método
    /// recibe el número (ej. 101), no el ID interno.
    /// </summary>
    Task AgregarHabitacionAsync(int reservaId, int numeroHabitacion, CancellationToken ct = default);

    /// <summary>Quita una habitación de una reserva pendiente.</summary>
    Task QuitarHabitacionAsync(int reservaId, int habitacionId, CancellationToken ct = default);

    /// <summary>Agrega un servicio adicional a una reserva pendiente.</summary>
    Task AgregarServicioAsync(int reservaId, int servicioAdicionalId, int cantidad, CancellationToken ct = default);

    /// <summary>Quita un servicio adicional de una reserva pendiente.</summary>
    Task QuitarServicioAsync(int reservaId, int servicioAdicionalId, CancellationToken ct = default);
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.UseCases.Reservas.AgregarHabitacion;
using SGRH.Application.UseCases.Reservas.AgregarServicio;
using SGRH.Application.UseCases.Reservas.CambiarFechas;
using SGRH.Application.UseCases.Reservas.CancelarReserva;
using SGRH.Application.UseCases.Reservas.ConfirmarReserva;
using SGRH.Application.UseCases.Reservas.CrearReserva;
using SGRH.Application.UseCases.Reservas.GetReserva;
using SGRH.Application.UseCases.Reservas.ListarReservas;
using SGRH.Application.UseCases.Reservas.QuitarHabitacion;
using SGRH.Application.UseCases.Reservas.QuitarServicio;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Enums;

namespace SGRH.Api.Controllers;

[Authorize]
public sealed class ReservasController : BaseApiController
{
    private readonly CrearReservaUseCase _crear;
    private readonly ListarReservasUseCase _listar;
    private readonly GetReservaUseCase _get;
    private readonly ConfirmarReservaUseCase _confirmar;
    private readonly CancelarReservaUseCase _cancelar;
    private readonly CambiarFechasUseCase _cambiarFechas;
    private readonly AgregarHabitacionUseCase _agregarHabitacion;
    private readonly AgregarServicioUseCase _agregarServicio;
    private readonly QuitarHabitacionUseCase _quitarHabitacion;
    private readonly QuitarServicioUseCase _quitarServicio;
    private readonly IHabitacionRepository _habitaciones;

    public ReservasController(
        CrearReservaUseCase crear,
        ListarReservasUseCase listar,
        GetReservaUseCase get,
        ConfirmarReservaUseCase confirmar,
        CancelarReservaUseCase cancelar,
        CambiarFechasUseCase cambiarFechas,
        AgregarHabitacionUseCase agregarHabitacion,
        AgregarServicioUseCase agregarServicio,
        QuitarHabitacionUseCase quitarHabitacion,
        QuitarServicioUseCase quitarServicio,
        IHabitacionRepository habitaciones)
    {
        _crear = crear;
        _listar = listar;
        _get = get;
        _confirmar = confirmar;
        _cancelar = cancelar;
        _cambiarFechas = cambiarFechas;
        _agregarHabitacion = agregarHabitacion;
        _agregarServicio = agregarServicio;
        _quitarHabitacion = quitarHabitacion;
        _quitarServicio = quitarServicio;
        _habitaciones = habitaciones;
    }

    // ── Helper: verifica que la reserva pertenezca al cliente autenticado ─
    private async Task<IActionResult?> ValidarAccesoClienteAsync(
        int reservaId, CancellationToken ct)
    {
        if (UsuarioActualRol != "CLIENTE") return null;

        var reserva = await _get.ExecuteAsync(reservaId, ct);
        if (reserva is null)
            return NotFoundProblem($"Reserva {reservaId} no encontrada.");

        if (reserva.Reserva.ClienteId != ClienteIdActual)
            return Problem(
                statusCode: 403,
                title: "Acceso denegado.",
                detail: "No tienes permiso para modificar una reserva que no es tuya.");

        return null;
    }

    /// <summary>Lista reservas. Admin/Recepcionista ven todas; Cliente solo las suyas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int? clienteId,
        [FromQuery] string? estado,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] DateTime? reservadaDesde,
        [FromQuery] DateTime? reservadaHasta,
        CancellationToken ct)
    {
        var filtroClienteId = UsuarioActualRol == "CLIENTE"
            ? ClienteIdActual
            : clienteId;

        EstadoReserva? estadoEnum = estado is not null
            && Enum.TryParse<EstadoReserva>(estado, ignoreCase: true, out var estadoParsed)
            ? estadoParsed : null;

        var response = await _listar.ExecuteAsync(
            filtroClienteId, estadoEnum, fechaDesde, fechaHasta, reservadaDesde, reservadaHasta, ct);
        return Ok(response);
    }

    /// <summary>Obtiene una reserva por ID. Cliente solo puede ver la suya.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        if (response is null)
            return NotFoundProblem($"Reserva {id} no encontrada.");

        if (UsuarioActualRol == "CLIENTE" && response.Reserva.ClienteId != ClienteIdActual)
            return Problem(statusCode: 403, title: "Acceso denegado.",
                detail: "No tienes permiso para ver una reserva que no es tuya.");

        return Ok(response);
    }

    /// <summary>
    /// Crea una reserva. Cliente la crea automáticamente para sí mismo.
    /// Admin/Recepcionista deben indicar el ClienteId.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearReservaBody body, CancellationToken ct)
    {
        var clienteId = UsuarioActualRol == "CLIENTE"
            ? ClienteIdActual!.Value
            : body.ClienteId ?? throw new ApplicationException(
                "ClienteId es requerido para Admin y Recepcionista.");

        var request = new CrearReservaRequest(
            clienteId, body.FechaEntrada, body.FechaSalida, BuildAuditInfo());
        var response = await _crear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return CreatedAtAction(nameof(Get), new { id = response.ReservaId }, response);
    }

    /// <summary>
    /// Confirma una reserva pendiente.
    /// Cliente solo puede confirmar la suya.
    /// </summary>
    [HttpPatch("{id:int}/confirmar")]
    public async Task<IActionResult> Confirmar(int id, CancellationToken ct)
    {
        var acceso = await ValidarAccesoClienteAsync(id, ct);
        if (acceso is not null) return acceso;

        await _confirmar.ExecuteAsync(
            id, UsuarioActualId, UsuarioActualRol, UsernameActual, BuildAuditInfo(), ct);
        return NoContent();
    }

    /// <summary>Cancela una reserva. Cliente solo puede cancelar la suya.</summary>
    [HttpPatch("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
    {
        var acceso = await ValidarAccesoClienteAsync(id, ct);
        if (acceso is not null) return acceso;

        await _cancelar.ExecuteAsync(
            id, UsuarioActualId, UsuarioActualRol, UsernameActual, BuildAuditInfo(), ct);
        return NoContent();
    }

    /// <summary>Cambia fechas de una reserva. Cliente solo puede cambiar la suya.</summary>
    [HttpPatch("{id:int}/fechas")]
    public async Task<IActionResult> CambiarFechas(
        int id, [FromBody] CambiarFechasBody body, CancellationToken ct)
    {
        var acceso = await ValidarAccesoClienteAsync(id, ct);
        if (acceso is not null) return acceso;

        var request = new CambiarFechasRequest(
            id, body.NuevaFechaEntrada, body.NuevaFechaSalida, BuildAuditInfo());
        await _cambiarFechas.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return NoContent();
    }

    /// <summary>
    /// Agrega una habitación a una reserva por número de habitación.
    /// El cliente usa el número visible (ej: 101), no el ID interno.
    /// Cliente solo puede modificar la suya.
    /// </summary>
    [HttpPost("{id:int}/habitaciones")]
    public async Task<IActionResult> AgregarHabitacion(
        int id, [FromBody] AgregarHabitacionBody body, CancellationToken ct)
    {
        var acceso = await ValidarAccesoClienteAsync(id, ct);
        if (acceso is not null) return acceso;

        // Resolver número de habitación → ID interno
        var habitacion = await _habitaciones.GetByNumeroAsync(body.NumeroHabitacion, ct);
        if (habitacion is null)
            return NotFoundProblem($"No existe una habitación con el número {body.NumeroHabitacion}.");

        var request = new AgregarHabitacionRequest(id, habitacion.HabitacionId, BuildAuditInfo());
        await _agregarHabitacion.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return NoContent();
    }

    /// <summary>
    /// Elimina una habitación de una reserva pendiente.
    /// Cliente solo puede modificar la suya.
    /// </summary>
    [HttpDelete("{id:int}/habitaciones/{habitacionId:int}")]
    public async Task<IActionResult> QuitarHabitacion(
        int id, int habitacionId, CancellationToken ct)
    {
        var acceso = await ValidarAccesoClienteAsync(id, ct);
        if (acceso is not null) return acceso;

        var request = new QuitarHabitacionRequest(id, habitacionId, BuildAuditInfo());
        await _quitarHabitacion.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return NoContent();
    }

    /// <summary>Agrega un servicio a una reserva. Cliente solo puede modificar la suya.</summary>
    [HttpPost("{id:int}/servicios")]
    public async Task<IActionResult> AgregarServicio(
        int id, [FromBody] AgregarServicioBody body, CancellationToken ct)
    {
        var acceso = await ValidarAccesoClienteAsync(id, ct);
        if (acceso is not null) return acceso;

        var request = new AgregarServicioRequest(
            id, body.ServicioAdicionalId, body.Cantidad, BuildAuditInfo());
        await _agregarServicio.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return NoContent();
    }

    /// <summary>
    /// Elimina un servicio de una reserva pendiente.
    /// Cliente solo puede modificar la suya.
    /// </summary>
    [HttpDelete("{id:int}/servicios/{servicioId:int}")]
    public async Task<IActionResult> QuitarServicio(
        int id, int servicioId, CancellationToken ct)
    {
        var acceso = await ValidarAccesoClienteAsync(id, ct);
        if (acceso is not null) return acceso;

        var request = new QuitarServicioRequest(id, servicioId, BuildAuditInfo());
        await _quitarServicio.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return NoContent();
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    public sealed record CrearReservaBody(int? ClienteId, DateTime FechaEntrada, DateTime FechaSalida);
    public sealed record CambiarFechasBody(DateTime NuevaFechaEntrada, DateTime NuevaFechaSalida);
    public sealed record AgregarHabitacionBody(int NumeroHabitacion);
    public sealed record AgregarServicioBody(int ServicioAdicionalId, int Cantidad);
}
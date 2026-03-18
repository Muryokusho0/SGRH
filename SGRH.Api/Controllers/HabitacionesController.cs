using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.UseCases.Habitaciones.BloquearHabitacion;
using SGRH.Application.UseCases.Habitaciones.CambiarEstadoHabitacion;
using SGRH.Application.UseCases.Habitaciones.CrearHabitacion;
using SGRH.Application.UseCases.Habitaciones.ListarHabitacionesDisponibles;
using SGRH.Application.UseCases.Habitaciones.GetHabitacion;
using SGRH.Application.UseCases.Habitaciones.ListarHabitaciones;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Api.Controllers;

[Authorize]
public sealed class HabitacionesController : BaseApiController
{
    private readonly CrearHabitacionUseCase _crear;
    private readonly ListarHabitacionesUseCase _listar;
    private readonly GetHabitacionUseCase _get;
    private readonly BloquearHabitacionUseCase _bloquear;
    private readonly CambiarEstadoHabitacionUseCase _cambiarEstado;
    private readonly ListarHabitacionesDisponiblesUseCase _disponibles;
    private readonly IReservaRepository _reservas;

    public HabitacionesController(
        CrearHabitacionUseCase crear,
        ListarHabitacionesUseCase listar,
        GetHabitacionUseCase get,
        BloquearHabitacionUseCase bloquear,
        CambiarEstadoHabitacionUseCase cambiarEstado,
        ListarHabitacionesDisponiblesUseCase disponibles,
        IReservaRepository reservas)
    {
        _crear = crear;
        _listar = listar;
        _get = get;
        _bloquear = bloquear;
        _cambiarEstado = cambiarEstado;
        _disponibles = disponibles;
        _reservas = reservas;
    }

    /// <summary>
    /// Lista todas las habitaciones con estado y historial. [AdminORecepcionista]
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminORecepcionista")]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] int? categoriaId,
        [FromQuery] int? piso,
        CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(estado, categoriaId, piso, ct);
        return Ok(response);
    }

    /// <summary>
    /// Habitaciones disponibles en un rango de fechas con rangos ocupados.
    /// Disponible para todos los usuarios autenticados. — RF-04
    /// </summary>
    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles(
        [FromQuery] DateTime entrada,
        [FromQuery] DateTime salida,
        [FromQuery] int? categoriaId,
        CancellationToken ct)
    {
        var request = new ListarHabitacionesDisponiblesRequest(entrada, salida, categoriaId);
        var response = await _disponibles.ExecuteAsync(request, ct);

        // Enriquecer cada habitación con los rangos en que estará ocupada
        var habitacionesConRangos = new List<object>();
        foreach (var hab in response.Habitaciones)
        {
            var rangos = await _reservas
                .GetRangosOcupadosPorHabitacionAsync(hab.HabitacionId, ct);

            habitacionesConRangos.Add(new
            {
                hab.HabitacionId,
                hab.NumeroHabitacion,
                hab.Piso,
                hab.NombreCategoria,
                hab.CategoriaHabitacionId,
                hab.EstadoActual,
                // Fechas en las que esta habitación estará ocupada (reservas activas futuras)
                PeriodosOcupados = rangos.Select(r => new
                {
                    Desde = r.FechaEntrada,
                    Hasta = r.FechaSalida,
                    Estado = r.Estado
                })
            });
        }

        return Ok(new { habitaciones = habitacionesConRangos });
    }

    /// <summary>Obtiene una habitación por ID. [AdminORecepcionista]</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AdminORecepcionista")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        return response is null ? NotFoundProblem($"Habitación {id} no encontrada.") : Ok(response);
    }

    /// <summary>
    /// Rangos de fechas en que una habitación estará ocupada.
    /// Útil para que el cliente sepa qué días no puede reservar. [Autenticado]
    /// </summary>
    [HttpGet("{id:int}/ocupacion")]
    public async Task<IActionResult> Ocupacion(int id, CancellationToken ct)
    {
        var rangos = await _reservas.GetRangosOcupadosPorHabitacionAsync(id, ct);
        return Ok(new
        {
            HabitacionId = id,
            PeriodosOcupados = rangos.Select(r => new
            {
                Desde = r.FechaEntrada,
                Hasta = r.FechaSalida,
                Estado = r.Estado
            })
        });
    }

    /// <summary>Crea una nueva habitación. [SoloAdmin]</summary>
    [HttpPost]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearHabitacionBody body, CancellationToken ct)
    {
        var request = new CrearHabitacionRequest(
            body.NumeroPiso, body.NumeroHabitacion, body.CategoriaHabitacionId,
            BuildAuditInfo());
        var response = await _crear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Habitacion.HabitacionId }, response);
    }

    /// <summary>Bloquea una habitación por mantenimiento. [AdminORecepcionista] — RF-07</summary>
    [HttpPatch("{id:int}/bloquear")]
    [Authorize(Policy = "AdminORecepcionista")]
    public async Task<IActionResult> Bloquear(
        int id, [FromBody] BloquearBody body, CancellationToken ct)
    {
        var request = new BloquearHabitacionRequest(id, body.Motivo, BuildAuditInfo());
        await _bloquear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return NoContent();
    }

    /// <summary>
    /// Cambia el estado de una habitación. [AdminORecepcionista] — RF-06
    /// Estados: Disponible, Ocupada, Mantenimiento (requiere motivo), Limpieza (requiere motivo).
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [Authorize(Policy = "AdminORecepcionista")]
    public async Task<IActionResult> CambiarEstado(
        int id, [FromBody] CambiarEstadoBody body, CancellationToken ct)
    {
        var request = new CambiarEstadoHabitacionRequest(
            id, body.NuevoEstado, body.Motivo, BuildAuditInfo());
        await _cambiarEstado.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return NoContent();
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    public sealed record CrearHabitacionBody(
        int NumeroPiso, int NumeroHabitacion, int CategoriaHabitacionId);
    public sealed record BloquearBody(string Motivo);
    public sealed record CambiarEstadoBody(string NuevoEstado, string? Motivo);
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.Abstractions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Temporadas;
using SGRH.Domain.Exceptions;
using SGRH.Application.UseCases.Temporadas.CrearTemporada;
using SGRH.Application.UseCases.Temporadas.GetTemporada;
using SGRH.Application.UseCases.Temporadas.ListarTemporadas;

namespace SGRH.Api.Controllers;

/// <summary>
/// Endpoints para administración y consulta de temporadas.
/// </summary>
[Authorize]
public sealed class TemporadasController : BaseApiController
{
    private readonly CrearTemporadaUseCase _crear;
    private readonly GetTemporadaUseCase _get;
    private readonly ListarTemporadasUseCase _listar;
    private readonly ITemporadaRepository _temporadas;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    /// <summary>Inicializa el controlador de temporadas.</summary>
    public TemporadasController(
        CrearTemporadaUseCase crear,
        GetTemporadaUseCase get,
        ListarTemporadasUseCase listar,
        ITemporadaRepository temporadas,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _crear = crear;
        _get = get;
        _listar = listar;
        _temporadas = temporadas;
        _uow = uow;
        _auditoria = auditoria;
    }

    /// <summary>Lista temporadas por filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nombre,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(nombre, fechaDesde, fechaHasta, ct);
        return Ok(response);
    }

    /// <summary>Obtiene una temporada por Id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        return response is null ? NotFoundProblem($"Temporada {id} no encontrada.") : Ok(response);
    }

    /// <summary>Crea una temporada específica por fecha. Requiere rol admin.</summary>
    [HttpPost]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearTemporadaBody body, CancellationToken ct)
    {
        var request = new CrearTemporadaRequest(
            body.NombreTemporada, body.FechaInicio, body.FechaFin, BuildAuditInfo());
        var response = await _crear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Temporada.TemporadaId }, response);
    }

    /// <summary>Crea una temporada recurrente (aplica cada año). Requiere rol admin.</summary>
    [HttpPost("recurrente")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> CrearRecurrente(
        [FromBody] CrearTemporadaRecurrenteBody body, CancellationToken ct)
    {
        var temporada = new Temporada(
            body.NombreTemporada,
            (byte)body.MesInicio, (byte)body.DiaInicio,
            (byte)body.MesFin, (byte)body.DiaFin);

        await _temporadas.AddAsync(temporada, ct);

        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "CREATE",
                modulo: "Temporadas",
                entidad: "Temporada",
                entidadId: temporada.TemporadaId.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: $"Temporada recurrente '{body.NombreTemporada}' creada " +
                                  $"({body.DiaInicio}/{body.MesInicio} – {body.DiaFin}/{body.MesFin})."), ct);

            await _uow.CommitAsync(ct);
        }
        catch { await _uow.RollbackAsync(ct); throw; }

        return CreatedAtAction(nameof(Get), new { id = temporada.TemporadaId },
            new
            {
                temporada.TemporadaId,
                temporada.NombreTemporada,
                temporada.EsRecurrente,
                temporada.MesInicio,
                temporada.DiaInicio,
                temporada.MesFin,
                temporada.DiaFin
            });
    }

    /// <summary>Actualiza parcialmente una temporada. Requiere rol admin.</summary>
    [HttpPatch("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> ModificarParcial(
        int id, [FromBody] PatchTemporadaBody? body, CancellationToken ct)
    {
        body ??= new PatchTemporadaBody(null, null, null, null, null, null, null);

        var temporada = await _temporadas.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Temporada", id.ToString());

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (temporada.EsRecurrente)
            {
                temporada.ActualizarRecurrente(
                    body.NombreTemporada ?? temporada.NombreTemporada,
                    (byte)(body.MesInicio ?? temporada.MesInicio!.Value),
                    (byte)(body.DiaInicio ?? temporada.DiaInicio!.Value),
                    (byte)(body.MesFin ?? temporada.MesFin!.Value),
                    (byte)(body.DiaFin ?? temporada.DiaFin!.Value));
            }
            else
            {
                temporada.Actualizar(
                    body.NombreTemporada ?? temporada.NombreTemporada,
                    body.FechaInicio ?? temporada.FechaInicio!.Value,
                    body.FechaFin ?? temporada.FechaFin!.Value);
            }

            _temporadas.Update(temporada);
            await _uow.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "UPDATE",
                modulo: "Temporadas",
                entidad: "Temporada",
                entidadId: id.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: $"Temporada {id} modificada."), ct);

            await _uow.CommitAsync(ct);
        }
        catch { await _uow.RollbackAsync(ct); throw; }

        return Ok(await _get.ExecuteAsync(id, ct));
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    /// <summary>Payload para crear temporada específica por fechas.</summary>
    public sealed record CrearTemporadaBody(
        string NombreTemporada, DateTime FechaInicio, DateTime FechaFin);

    /// <summary>Payload para crear temporadas recurrentes por mes/día.</summary>
    public sealed record CrearTemporadaRecurrenteBody(
        string NombreTemporada,
        int MesInicio, int DiaInicio,
        int MesFin, int DiaFin);

    /// <summary>Payload para patch de temporada (específica o recurrente).</summary>
    public sealed record PatchTemporadaBody(
        string? NombreTemporada,
        DateTime? FechaInicio, DateTime? FechaFin,
        int? MesInicio, int? DiaInicio,
        int? MesFin, int? DiaFin);
}
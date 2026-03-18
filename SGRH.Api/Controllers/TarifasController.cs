using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using SGRH.Application.UseCases.Tarifas.CrearTarifa;
using SGRH.Application.UseCases.Tarifas.GetTarifa;
using SGRH.Application.UseCases.Tarifas.ListarTarifas;

namespace SGRH.Api.Controllers;

[Authorize(Policy = "SoloAdmin")]
public sealed class TarifasController : BaseApiController
{
    private readonly CrearTarifaUseCase _crear;
    private readonly GetTarifaUseCase _get;
    private readonly ListarTarifasUseCase _listar;
    private readonly ITarifaTemporadaRepository _tarifas;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public TarifasController(
        CrearTarifaUseCase crear,
        GetTarifaUseCase get,
        ListarTarifasUseCase listar,
        ITarifaTemporadaRepository tarifas,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _crear = crear;
        _get = get;
        _listar = listar;
        _tarifas = tarifas;
        _uow = uow;
        _auditoria = auditoria;
    }

    /// <summary>Lista tarifas con filtros opcionales. [SoloAdmin]</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int? categoriaId,
        [FromQuery] int? temporadaId,
        CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(categoriaId, temporadaId, ct);
        return Ok(response);
    }

    /// <summary>Obtiene una tarifa por ID. [SoloAdmin]</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        return response is null ? NotFoundProblem($"Tarifa {id} no encontrada.") : Ok(response);
    }

    /// <summary>Crea una tarifa por temporada y categoría. [SoloAdmin]</summary>
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearTarifaBody body, CancellationToken ct)
    {
        var request = new CrearTarifaRequest(
            body.CategoriaHabitacionId, body.TemporadaId, body.PrecioNoche, BuildAuditInfo());
        var response = await _crear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Tarifa.TarifaTemporadaId }, response);
    }

    /// <summary>
    /// Actualiza el precio de una tarifa existente. [SoloAdmin]
    /// Solo envía el campo que quieres cambiar: { "precioNoche": 150.00 }
    /// </summary>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ModificarParcial(
        int id, [FromBody] PatchTarifaBody? body, CancellationToken ct)
    {
        if (body?.PrecioNoche is null)
            return Problem(statusCode: 400, detail: "Debes enviar al menos precioNoche.");

        var tarifa = await _tarifas.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("TarifaTemporada", id.ToString());

        var precioAnterior = tarifa.Precio;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            tarifa.ActualizarPrecio(body.PrecioNoche.Value);
            _tarifas.Update(tarifa);
            await _uow.SaveChangesAsync(ct);

            var evento = new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "UPDATE",
                modulo: "Tarifas",
                entidad: "TarifaTemporada",
                entidadId: id.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: $"Precio de tarifa {id} actualizado.");

            evento.AgregarDetalle("Precio",
                precioAnterior.ToString("F2"),
                body.PrecioNoche.Value.ToString("F2"));

            await _auditoria.RegistrarAsync(evento, ct);
            await _uow.CommitAsync(ct);
        }
        catch { await _uow.RollbackAsync(ct); throw; }

        var actualizada = await _get.ExecuteAsync(id, ct);
        return Ok(actualizada);
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    public sealed record CrearTarifaBody(int CategoriaHabitacionId, int TemporadaId, decimal PrecioNoche);
    public sealed record PatchTarifaBody(decimal? PrecioNoche);
}
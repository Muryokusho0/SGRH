using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using SGRH.Application.UseCases.Servicios.AsignarPrecioCategoria;
using SGRH.Application.UseCases.Servicios.AsignarServicioTemporada;
using SGRH.Application.UseCases.Servicios.CrearServicio;
using SGRH.Application.UseCases.Servicios.GetServicio;
using SGRH.Application.UseCases.Servicios.ListarServicios;
using SGRH.Application.UseCases.Servicios.ListarServiciosPorCategoria;

namespace SGRH.Api.Controllers;

[Authorize]
public sealed class ServiciosController : BaseApiController
{
    private readonly CrearServicioUseCase _crear;
    private readonly GetServicioUseCase _get;
    private readonly ListarServiciosUseCase _listar;
    private readonly ListarServiciosPorCategoriaUseCase _porCategoria;
    private readonly AsignarPrecioCategoriaUseCase _asignarPrecio;
    private readonly AsignarServicioTemporadaUseCase _asignarTemporada;
    private readonly IServicioAdicionalRepository _servicios;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public ServiciosController(
        CrearServicioUseCase crear,
        GetServicioUseCase get,
        ListarServiciosUseCase listar,
        ListarServiciosPorCategoriaUseCase porCategoria,
        AsignarPrecioCategoriaUseCase asignarPrecio,
        AsignarServicioTemporadaUseCase asignarTemporada,
        IServicioAdicionalRepository servicios,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _crear = crear;
        _get = get;
        _listar = listar;
        _porCategoria = porCategoria;
        _asignarPrecio = asignarPrecio;
        _asignarTemporada = asignarTemporada;
        _servicios = servicios;
        _uow = uow;
        _auditoria = auditoria;
    }

    /// <summary>Lista servicios. Disponible para todos los usuarios autenticados.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nombre, CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(nombre, ct);
        return Ok(response);
    }

    /// <summary>Lista servicios disponibles para una categoría con su precio aplicable.</summary>
    [HttpGet("por-categoria/{categoriaId:int}")]
    public async Task<IActionResult> PorCategoria(int categoriaId, CancellationToken ct)
    {
        var response = await _porCategoria.ExecuteAsync(categoriaId, ct);
        return Ok(response);
    }

    /// <summary>Obtiene un servicio por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        return response is null ? NotFoundProblem($"Servicio {id} no encontrado.") : Ok(response);
    }

    /// <summary>Crea un nuevo servicio adicional. [SoloAdmin]</summary>
    [HttpPost]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearServicioBody body, CancellationToken ct)
    {
        var request = new CrearServicioRequest(body.NombreServicio, body.TipoServicio, BuildAuditInfo());
        var response = await _crear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Servicio.ServicioAdicionalId }, response);
    }

    /// <summary>
    /// Actualiza nombre y/o tipo de un servicio. Campos omitidos conservan valor actual. [SoloAdmin]
    /// </summary>
    [HttpPatch("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> ModificarParcial(
        int id, [FromBody] PatchServicioBody? body, CancellationToken ct)
    {
        body ??= new PatchServicioBody(null, null);

        var servicio = await _servicios.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("ServicioAdicional", id.ToString());

        var nuevoNombre = body.NombreServicio ?? servicio.NombreServicio;
        var nuevoTipo = body.TipoServicio ?? servicio.TipoServicio;

        var nombreAnterior = servicio.NombreServicio;
        var tipoAnterior = servicio.TipoServicio;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            servicio.Actualizar(nuevoNombre, nuevoTipo);
            _servicios.Update(servicio);
            await _uow.SaveChangesAsync(ct);

            var evento = new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "UPDATE",
                modulo: "Servicios",
                entidad: "ServicioAdicional",
                entidadId: id.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: $"Servicio {id} modificado.");

            if (!string.Equals(nombreAnterior, nuevoNombre, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("NombreServicio", nombreAnterior, nuevoNombre);
            if (!string.Equals(tipoAnterior, nuevoTipo, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("TipoServicio", tipoAnterior, nuevoTipo);

            await _auditoria.RegistrarAsync(evento, ct);
            await _uow.CommitAsync(ct);
        }
        catch { await _uow.RollbackAsync(ct); throw; }

        return Ok(new { servicio = servicio.ToDto() });
    }

    /// <summary>Asigna o actualiza el precio de un servicio para una categoría. [SoloAdmin]</summary>
    [HttpPost("precio-categoria")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> AsignarPrecioCategoria(
        [FromBody] AsignarPrecioCategoriaBody body, CancellationToken ct)
    {
        var request = new AsignarPrecioCategoriaRequest(
            body.ServicioAdicionalId, body.CategoriaHabitacionId, body.Precio, BuildAuditInfo());
        var response = await _asignarPrecio.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return Ok(response);
    }

    /// <summary>Asigna un servicio a una temporada. [SoloAdmin]</summary>
    [HttpPost("temporada")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> AsignarTemporada(
        [FromBody] AsignarServicioTemporadaBody body, CancellationToken ct)
    {
        var request = new AsignarServicioTemporadaRequest(
            body.ServicioAdicionalId, body.TemporadaId, BuildAuditInfo());
        var response = await _asignarTemporada.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return Ok(response);
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    public sealed record CrearServicioBody(string NombreServicio, string TipoServicio);
    public sealed record PatchServicioBody(string? NombreServicio, string? TipoServicio);
    public sealed record AsignarPrecioCategoriaBody(
        int ServicioAdicionalId, int CategoriaHabitacionId, decimal Precio);
    public sealed record AsignarServicioTemporadaBody(
        int ServicioAdicionalId, int TemporadaId);

    /// <summary>
    /// Marca el servicio como disponible en TODAS las temporadas.
    /// Ya no es necesario asignarlo temporada por temporada. [SoloAdmin]
    /// </summary>
    [HttpPost("{id:int}/todas-temporadas")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> HabilitarTodasTemporadas(int id, CancellationToken ct)
    {
        var servicio = await _servicios.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("ServicioAdicional", id.ToString());

        await _uow.BeginTransactionAsync(ct);
        try
        {
            servicio.HabilitarParaTodasTemporadas();
            _servicios.Update(servicio);
            await _uow.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "UPDATE",
                modulo: "Servicios",
                entidad: "ServicioAdicional",
                entidadId: id.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: $"Servicio {id} habilitado para todas las temporadas."), ct);

            await _uow.CommitAsync(ct);
        }
        catch { await _uow.RollbackAsync(ct); throw; }

        return Ok(new { mensaje = "Servicio habilitado para todas las temporadas.", servicio = servicio.ToDto() });
    }

    /// <summary>
    /// Revierte el servicio a modo por temporada.
    /// Deberás asignarlo manualmente a cada temporada. [SoloAdmin]
    /// </summary>
    [HttpDelete("{id:int}/todas-temporadas")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> DeshabilitarTodasTemporadas(int id, CancellationToken ct)
    {
        var servicio = await _servicios.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("ServicioAdicional", id.ToString());

        await _uow.BeginTransactionAsync(ct);
        try
        {
            servicio.DeshabilitarParaTodasTemporadas();
            _servicios.Update(servicio);
            await _uow.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "UPDATE",
                modulo: "Servicios",
                entidad: "ServicioAdicional",
                entidadId: id.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: $"Servicio {id} revertido a modo por temporada."), ct);

            await _uow.CommitAsync(ct);
        }
        catch { await _uow.RollbackAsync(ct); throw; }

        return Ok(new { mensaje = "Servicio revertido a modo por temporada.", servicio = servicio.ToDto() });
    }


}
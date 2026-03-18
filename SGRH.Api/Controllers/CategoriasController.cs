using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.UseCases.Categorias.CrearCategoria;
using SGRH.Application.UseCases.Categorias.GetCategoria;
using SGRH.Application.UseCases.Categorias.ListarCategorias;
using SGRH.Application.UseCases.Categorias.ModificarCategoria;

namespace SGRH.Api.Controllers;

[Authorize]
public sealed class CategoriasController : BaseApiController
{
    private readonly CrearCategoriaUseCase _crear;
    private readonly ModificarCategoriaUseCase _modificar;
    private readonly GetCategoriaUseCase _get;
    private readonly ListarCategoriasUseCase _listar;

    public CategoriasController(
        CrearCategoriaUseCase crear,
        ModificarCategoriaUseCase modificar,
        GetCategoriaUseCase get,
        ListarCategoriasUseCase listar)
    {
        _crear = crear;
        _modificar = modificar;
        _get = get;
        _listar = listar;
    }

    /// <summary>
    /// Lista todas las categorías. Disponible para todos los usuarios autenticados.
    /// El cliente puede ver categorías para elegir al buscar habitaciones.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nombre,
        [FromQuery] int? capacidadMinima,
        [FromQuery] int? capacidadMaxima,
        CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(nombre, capacidadMinima, capacidadMaxima, ct);
        return Ok(response);
    }

    /// <summary>Obtiene una categoría por ID. Disponible para todos los usuarios autenticados.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        return response is null
            ? NotFoundProblem($"Categoría {id} no encontrada.")
            : Ok(response);
    }

    /// <summary>Crea una nueva categoría. [SoloAdmin]</summary>
    [HttpPost]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearCategoriaBody body, CancellationToken ct)
    {
        var request = new CrearCategoriaRequest(
            body.NombreCategoria, body.Capacidad, body.Descripcion, body.PrecioBase,
            BuildAuditInfo());
        var response = await _crear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return CreatedAtAction(
            nameof(Get),
            new { id = response.Categoria.CategoriaHabitacionId },
            response);
    }

    /// <summary>Reemplaza una categoría completa. Todos los campos requeridos. [SoloAdmin]</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Modificar(
        int id, [FromBody] ModificarCategoriaBody body, CancellationToken ct)
    {
        var request = new ModificarCategoriaRequest(
            id, body.NombreCategoria, body.Capacidad, body.Descripcion, body.PrecioBase,
            BuildAuditInfo());
        var response = await _modificar.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return Ok(response);
    }

    /// <summary>
    /// Actualiza solo los campos enviados. Los omitidos conservan su valor. [SoloAdmin]
    /// Ejemplo: { "precioBase": 150.00 }
    /// </summary>
    [HttpPatch("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> ModificarParcial(
        int id, [FromBody] PatchCategoriaBody? body, CancellationToken ct)
    {
        body ??= new PatchCategoriaBody(null, null, null, null);

        var actual = await _get.ExecuteAsync(id, ct);
        if (actual is null) return NotFoundProblem($"Categoría {id} no encontrada.");

        var request = new ModificarCategoriaRequest(
            CategoriaHabitacionId: id,
            NombreCategoria: body.NombreCategoria ?? actual.Categoria.NombreCategoria,
            Capacidad: body.Capacidad ?? actual.Categoria.Capacidad,
            Descripcion: body.Descripcion ?? actual.Categoria.Descripcion,
            PrecioBase: body.PrecioBase ?? actual.Categoria.PrecioBase,
            AuditInfo: BuildAuditInfo());

        var response = await _modificar.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return Ok(response);
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    public sealed record CrearCategoriaBody(
        string NombreCategoria, int Capacidad, string Descripcion, decimal PrecioBase);
    public sealed record ModificarCategoriaBody(
        string NombreCategoria, int Capacidad, string? Descripcion, decimal PrecioBase);
    public sealed record PatchCategoriaBody(
        string? NombreCategoria, int? Capacidad, string? Descripcion, decimal? PrecioBase);
}
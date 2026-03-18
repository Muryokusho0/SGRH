using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.UseCases.Usuarios.CrearUsuario;
using SGRH.Application.UseCases.Usuarios.DesactivarUsuario;
using SGRH.Application.UseCases.Usuarios.GetUsuario;
using SGRH.Application.UseCases.Usuarios.ListarUsuarios;

namespace SGRH.Api.Controllers;

[Authorize(Policy = "SoloAdmin")]
public sealed class UsuariosController : BaseApiController
{
    private readonly CrearUsuarioUseCase _crear;
    private readonly DesactivarUsuarioUseCase _desactivar;
    private readonly GetUsuarioUseCase _get;
    private readonly ListarUsuariosUseCase _listar;

    public UsuariosController(
        CrearUsuarioUseCase crear,
        DesactivarUsuarioUseCase desactivar,
        GetUsuarioUseCase get,
        ListarUsuariosUseCase listar)
    {
        _crear = crear;
        _desactivar = desactivar;
        _get = get;
        _listar = listar;
    }

    /// <summary>Lista todos los usuarios con filtros opcionales. [SoloAdmin]</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? rol,
        [FromQuery] bool? activo,
        CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(rol, activo, ct);
        return Ok(response);
    }

    /// <summary>Obtiene un usuario por ID. [SoloAdmin]</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        return response is null ? NotFoundProblem($"Usuario {id} no encontrado.") : Ok(response);
    }

    /// <summary>Crea un usuario Admin o Recepcionista. [SoloAdmin]</summary>
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUsuarioBody body, CancellationToken ct)
    {
        // CrearUsuarioRequest: Email, Password, ConfirmPassword, Rol, AuditInfo
        var request = new CrearUsuarioRequest(
            body.Email, body.Password, body.ConfirmPassword, body.Rol, BuildAuditInfo());
        var response = await _crear.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Usuario.UsuarioId }, response);
    }

    /// <summary>Desactiva un usuario. [SoloAdmin]</summary>
    [HttpPatch("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        await _desactivar.ExecuteAsync(
            id, UsuarioActualId, UsuarioActualRol, UsernameActual, BuildAuditInfo(), ct);
        return NoContent();
    }

    // ── Bodies ───────────────────────────────────────────────────────────

    public sealed record CrearUsuarioBody(
        string Email, string Password, string ConfirmPassword, string Rol);
}
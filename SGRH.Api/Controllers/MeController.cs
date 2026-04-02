using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Auth;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

namespace SGRH.Api.Controllers;

/// <summary>
/// Endpoints del perfil del usuario autenticado.
/// </summary>
[Authorize]
[Route("api/me")]
[ApiController]
public sealed class MeController : BaseApiController
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IClienteRepository _clientes;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    /// <summary>Inicializa el controlador de perfil.</summary>
    public MeController(
        IUsuarioRepository usuarios,
        IClienteRepository clientes,
        IPasswordHasher hasher,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _usuarios = usuarios;
        _clientes = clientes;
        _hasher = hasher;
        _uow = uow;
        _auditoria = auditoria;
    }

    /// <summary>
    /// Devuelve el perfil del usuario autenticado y, si aplica, los datos del cliente.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet]
    public async Task<IActionResult> GetPerfil(CancellationToken ct)
    {
        var usuario = await _usuarios.GetByIdAsync(UsuarioActualId, ct)
            ?? throw new NotFoundException("Usuario", UsuarioActualId.ToString());

        if (UsuarioActualRol == "CLIENTE" && ClienteIdActual.HasValue)
        {
            var cliente = await _clientes.GetByIdAsync(ClienteIdActual.Value, ct);
            return Ok(new
            {
                usuario = usuario.ToDto(),
                cliente = cliente?.ToDto()
            });
        }

        return Ok(new { usuario = usuario.ToDto() });
    }

    /// <summary>Actualiza parcialmente el perfil del cliente autenticado.</summary>
    /// <param name="body">Campos opcionales a modificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPatch]
    [Authorize(Policy = "SoloCliente")]
    public async Task<IActionResult> ModificarPerfil(
        [FromBody] ModificarPerfilBody? body, CancellationToken ct)
    {
        body ??= new ModificarPerfilBody(null, null, null, null);

        if (!ClienteIdActual.HasValue)
            return Problem(statusCode: 400, detail: "No se encontró el ClienteId en el token.");

        var cliente = await _clientes.GetByIdAsync(ClienteIdActual.Value, ct)
            ?? throw new NotFoundException("Cliente", ClienteIdActual.Value.ToString());

        // Fusionar — solo actualiza los campos enviados
        var nuevoNombre = body.NombreCliente ?? cliente.NombreCliente;
        var nuevoApellido = body.ApellidoCliente ?? cliente.ApellidoCliente;
        var nuevoEmail = body.Email ?? cliente.Email;
        var nuevoTelefono = body.Telefono ?? cliente.Telefono;

        // Verificar unicidad de email si cambió
        if (!string.Equals(cliente.Email, nuevoEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existente = await _clientes.GetByEmailAsync(nuevoEmail, ct);
            if (existente is not null && existente.ClienteId != cliente.ClienteId)
                throw new ConflictException($"El email '{nuevoEmail}' ya está en uso.");
        }

        // Snapshots para auditoría
        var nombreAnterior = cliente.NombreCliente;
        var apellidoAnterior = cliente.ApellidoCliente;
        var emailAnterior = cliente.Email;
        var telefonoAnterior = cliente.Telefono;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            cliente.ActualizarDatos(nuevoNombre, nuevoApellido, nuevoEmail, nuevoTelefono);
            await _uow.SaveChangesAsync(ct);

            var evento = new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "UPDATE",
                modulo: "Clientes",
                entidad: "Cliente",
                entidadId: cliente.ClienteId.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: "Cliente actualizó su propio perfil.");

            if (!string.Equals(nombreAnterior, nuevoNombre, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("NombreCliente", nombreAnterior, nuevoNombre);
            if (!string.Equals(apellidoAnterior, nuevoApellido, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("ApellidoCliente", apellidoAnterior, nuevoApellido);
            if (!string.Equals(emailAnterior, nuevoEmail, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("Email", emailAnterior, nuevoEmail);
            if (!string.Equals(telefonoAnterior, nuevoTelefono, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("Telefono", telefonoAnterior, nuevoTelefono);

            await _auditoria.RegistrarAsync(evento, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }

        var clienteActualizado = await _clientes.GetByIdAsync(ClienteIdActual.Value, ct);
        return Ok(new { cliente = clienteActualizado?.ToDto() });
    }

    /// <summary>Cambia la contraseña del usuario autenticado.</summary>
    /// <param name="body">Credenciales actuales y nueva contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPost("password")]
    public async Task<IActionResult> CambiarPassword(
        [FromBody] CambiarPasswordBody body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.PasswordActual))
            return Problem(statusCode: 400, detail: "La contraseña actual es requerida.");

        if (string.IsNullOrWhiteSpace(body.NuevoPassword))
            return Problem(statusCode: 400, detail: "La nueva contraseña es requerida.");

        if (body.NuevoPassword.Length < 8)
            return Problem(statusCode: 400, detail: "La nueva contraseña debe tener al menos 8 caracteres.");

        if (body.NuevoPassword != body.ConfirmarPassword)
            return Problem(statusCode: 400, detail: "La nueva contraseña y la confirmación no coinciden.");

        var usuario = await _usuarios.GetByIdAsync(UsuarioActualId, ct)
            ?? throw new NotFoundException("Usuario", UsuarioActualId.ToString());

        // Verificar contraseña actual
        if (!_hasher.Verify(body.PasswordActual, usuario.PasswordHash))
            return Problem(statusCode: 400, detail: "La contraseña actual es incorrecta.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            usuario.CambiarPassword(_hasher.Hash(body.NuevoPassword));
            await _uow.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: UsuarioActualId,
                rol: UsuarioActualRol,
                usernameSnapshot: UsernameActual,
                accion: "UPDATE",
                modulo: "Auth",
                entidad: "Usuario",
                entidadId: UsuarioActualId.ToString(),
                requestId: BuildAuditInfo().RequestId,
                ipOrigen: BuildAuditInfo().IpOrigen,
                userAgent: BuildAuditInfo().UserAgent,
                descripcion: "Usuario cambió su contraseña."), ct);

            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }

        return Ok(new { mensaje = "Contraseña actualizada exitosamente." });
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    /// <summary>Payload de actualización parcial del perfil.</summary>
    public sealed record ModificarPerfilBody(
        string? NombreCliente,
        string? ApellidoCliente,
        string? Email,
        string? Telefono);

    /// <summary>Payload para cambiar la contraseña.</summary>
    public sealed record CambiarPasswordBody(
        string PasswordActual,
        string NuevoPassword,
        string ConfirmarPassword);
}
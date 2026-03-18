using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.UseCases.Clientes.GetCliente;
using SGRH.Application.UseCases.Clientes.ListarClientes;
using SGRH.Application.UseCases.Clientes.ModificarCliente;

namespace SGRH.Api.Controllers;

[Authorize(Policy = "AdminORecepcionista")]
public sealed class ClientesController : BaseApiController
{
    private readonly ListarClientesUseCase _listar;
    private readonly GetClienteUseCase _get;
    private readonly ModificarClienteUseCase _modificar;

    public ClientesController(
        ListarClientesUseCase listar,
        GetClienteUseCase get,
        ModificarClienteUseCase modificar)
    {
        _listar = listar;
        _get = get;
        _modificar = modificar;
    }

    /// <summary>Lista clientes con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nombre,
        [FromQuery] string? email,
        [FromQuery] string? nationalId,
        CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(nombre, email, nationalId, ct);
        return Ok(response);
    }

    /// <summary>Obtiene un cliente por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var response = await _get.ExecuteAsync(id, ct);
        return response is null ? NotFoundProblem($"Cliente {id} no encontrado.") : Ok(response);
    }

    /// <summary>Reemplaza todos los datos del cliente. Todos los campos son requeridos.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modificar(
        int id, [FromBody] ModificarClienteBody body, CancellationToken ct)
    {
        var request = new ModificarClienteRequest(
            id, body.NombreCliente!, body.ApellidoCliente!, body.Email!, body.Telefono!,
            BuildAuditInfo());
        var response = await _modificar.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return Ok(response);
    }

    /// <summary>
    /// Actualiza solo los campos enviados. Los omitidos conservan su valor actual.
    /// </summary>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ModificarParcial(
        int id, [FromBody] PatchClienteBody? body, CancellationToken ct)
    {
        body ??= new PatchClienteBody(null, null, null, null);

        var actual = await _get.ExecuteAsync(id, ct);
        if (actual is null)
            return NotFoundProblem($"Cliente {id} no encontrado.");

        var request = new ModificarClienteRequest(
            ClienteId: id,
            NombreCliente: body.NombreCliente ?? actual.Cliente.NombreCliente,
            ApellidoCliente: body.ApellidoCliente ?? actual.Cliente.ApellidoCliente,
            Email: body.Email ?? actual.Cliente.Email,
            Telefono: body.Telefono ?? actual.Cliente.Telefono,
            AuditInfo: BuildAuditInfo());

        var response = await _modificar.ExecuteAsync(
            request, UsuarioActualId, UsuarioActualRol, UsernameActual, ct);
        return Ok(response);
    }

    // ── Bodies ────────────────────────────────────────────────────────────

    public sealed record ModificarClienteBody(
        string? NombreCliente, string? ApellidoCliente, string? Email, string? Telefono);

    /// <summary>PATCH — todos los campos opcionales.</summary>
    public sealed record PatchClienteBody(
        string? NombreCliente, string? ApellidoCliente, string? Email, string? Telefono);
}
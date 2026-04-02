using Microsoft.AspNetCore.Mvc;
using SGRH.Application.Abstractions;
using System.Security.Claims;

namespace SGRH.Api.Controllers;

/// <summary>
/// Controlador base con utilidades comunes para autenticación, auditoría y respuestas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // ── Claims del token JWT ──────────────────────────────────────────────

    /// <summary>Id del usuario autenticado obtenido del JWT.</summary>
    protected int UsuarioActualId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Claim UsuarioId no encontrado."));

    /// <summary>Rol del usuario autenticado.</summary>
    protected string UsuarioActualRol =>
        User.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("Claim Rol no encontrado.");

    /// <summary>Nombre de usuario autenticado.</summary>
    protected string UsernameActual =>
        User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("unique_name")
            ?? throw new InvalidOperationException("Claim Username no encontrado.");

    /// <summary>
    /// Id del cliente asociado al usuario cuando el rol es CLIENTE.
    /// </summary>
    protected int? ClienteIdActual
    {
        get
        {
            var v = User.FindFirstValue("clienteId");
            return v is not null ? int.Parse(v) : null;
        }
    }

    // ── AuditInfo para UseCases de mutación ──────────────────────────────

    /// <summary>Construye el objeto de auditoría desde el contexto HTTP.</summary>
    /// <returns>Instancia de <see cref="AuditInfo"/> con requestId, IP y user-agent.</returns>
    protected AuditInfo BuildAuditInfo()
    {
        var requestId = HttpContext.TraceIdentifier is { Length: > 0 } tid
            ? (Guid.TryParse(tid, out var g) ? g : Guid.NewGuid())
            : Guid.NewGuid();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        return new AuditInfo(requestId, ip, userAgent);
    }

    // ── Helpers de respuesta ─────────────────────────────────────────────

    /// <summary>Devuelve 404 con un payload de error estándar.</summary>
    protected IActionResult NotFoundProblem(string mensaje) =>
        NotFound(new { error = mensaje });

    /// <summary>Devuelve 409 con un payload de error estándar.</summary>
    protected IActionResult ConflictProblem(string mensaje) =>
        Conflict(new { error = mensaje });

    /// <summary>Devuelve 400 con una lista de errores de validación.</summary>
    protected IActionResult ValidationProblem(IEnumerable<string> errores) =>
        BadRequest(new { errors = errores });
}
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.Abstractions;
using System.Security.Claims;

namespace SGRH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // ── Claims del token JWT ──────────────────────────────────────────────

    protected int UsuarioActualId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Claim UsuarioId no encontrado."));

    protected string UsuarioActualRol =>
        User.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("Claim Rol no encontrado.");

    protected string UsernameActual =>
        User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("unique_name")
            ?? throw new InvalidOperationException("Claim Username no encontrado.");

    // Disponible solo para rol CLIENTE — null para Admin y Recepcionista
    protected int? ClienteIdActual
    {
        get
        {
            var v = User.FindFirstValue("clienteId");
            return v is not null ? int.Parse(v) : null;
        }
    }

    // ── AuditInfo para UseCases de mutación ──────────────────────────────

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

    protected IActionResult NotFoundProblem(string mensaje) =>
        NotFound(new { error = mensaje });

    protected IActionResult ConflictProblem(string mensaje) =>
        Conflict(new { error = mensaje });

    protected IActionResult ValidationProblem(IEnumerable<string> errores) =>
        BadRequest(new { errors = errores });
}
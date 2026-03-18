using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Application.UseCases.Auditoria;

namespace SGRH.Api.Controllers;

[Authorize(Policy = "SoloAdmin")]
public sealed class AuditoriaController : BaseApiController
{
    private readonly ListarAuditoriaUseCase _listar;

    public AuditoriaController(ListarAuditoriaUseCase listar)
    {
        _listar = listar;
    }

    /// <summary>Lista eventos de auditoría con filtros opcionales. [SoloAdmin] — RF-15</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? modulo,
        [FromQuery] string? accion,
        [FromQuery] string? entidad,
        [FromQuery] int? usuarioId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken ct)
    {
        var response = await _listar.ExecuteAsync(
            modulo, accion, entidad, usuarioId, fechaDesde, fechaHasta, ct);
        return Ok(response);
    }
}
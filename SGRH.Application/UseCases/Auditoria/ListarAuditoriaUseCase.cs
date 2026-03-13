using SGRH.Application.Dtos.Auditoria;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Auditoria;

public sealed class ListarAuditoriaUseCase
{
    private readonly IAuditoriaRepository _auditoria;

    public ListarAuditoriaUseCase(IAuditoriaRepository auditoria)
    {
        _auditoria = auditoria;
    }

    public async Task<ListarAuditoriaResponse> ExecuteAsync(
        string? modulo = null,
        string? accion = null,
        string? entidad = null,
        int? usuarioId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default)
    {
        var eventos = await _auditoria.BuscarAsync(
            modulo, accion, entidad, usuarioId, fechaDesde, fechaHasta, ct);

        var dtos = eventos.Select(e => e.ToDto()).ToList();

        return new ListarAuditoriaResponse(dtos);
    }
}
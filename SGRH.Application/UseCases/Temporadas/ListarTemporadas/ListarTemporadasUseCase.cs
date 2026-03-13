using SGRH.Application.Dtos.Temporadas;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Temporadas.ListarTemporadas;

public sealed class ListarTemporadasUseCase
{
    private readonly ITemporadaRepository _temporadas;

    public ListarTemporadasUseCase(ITemporadaRepository temporadas)
    {
        _temporadas = temporadas;
    }

    public async Task<ListarTemporadasResponse> ExecuteAsync(
        string? nombre = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default)
    {
        var temporadas = await _temporadas.BuscarAsync(nombre, fechaDesde, fechaHasta, ct);

        var dtos = temporadas.Select(t => t.ToDto()).ToList();

        return new ListarTemporadasResponse(dtos);
    }
}
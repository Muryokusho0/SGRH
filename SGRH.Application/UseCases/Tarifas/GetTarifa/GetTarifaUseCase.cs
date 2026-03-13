using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Tarifas.GetTarifa;

public sealed class GetTarifaUseCase
{
    private readonly ITarifaTemporadaRepository _tarifas;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly ITemporadaRepository _temporadas;

    public GetTarifaUseCase(
        ITarifaTemporadaRepository tarifas,
        ICategoriaHabitacionRepository categorias,
        ITemporadaRepository temporadas)
    {
        _tarifas = tarifas;
        _categorias = categorias;
        _temporadas = temporadas;
    }

    public async Task<GetTarifaResponse> ExecuteAsync(
        int tarifaId, CancellationToken ct = default)
    {
        var tarifa = await _tarifas.GetByIdAsync(tarifaId, ct)
            ?? throw new NotFoundException("TarifaTemporada", tarifaId.ToString());

        var categoria = await _categorias.GetByIdAsync(tarifa.CategoriaHabitacionId, ct);
        var temporada = await _temporadas.GetByIdAsync(tarifa.TemporadaId, ct);

        return new GetTarifaResponse(TarifaMapper.ToDto(
            tarifa,
            categoria?.NombreCategoria ?? string.Empty,
            temporada?.NombreTemporada ?? string.Empty));
    }
}
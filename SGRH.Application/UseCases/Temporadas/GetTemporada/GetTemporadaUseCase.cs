using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using SGRH.Application.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.GetTemporada;

public sealed class GetTemporadaUseCase
{
    private readonly ITemporadaRepository _temporadas;

    public GetTemporadaUseCase(ITemporadaRepository temporadas)
    {
        _temporadas = temporadas;
    }

    public async Task<GetTemporadaResponse> ExecuteAsync(
        int temporadaId, CancellationToken ct = default)
    {
        var temporada = await _temporadas.GetByIdAsync(temporadaId, ct)
            ?? throw new NotFoundException("Temporada", temporadaId.ToString());

        return new GetTemporadaResponse(temporada.ToDto());
    }
}
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Application.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.GetTemporadaVigente;

// Consulta puntual: ¿qué temporada está activa para una fecha dada?
// Diferente a ListarTemporadas — aquí se busca la UNA temporada que contiene esa fecha.
// FechaFin es exclusiva: rango [FechaInicio, FechaFin).
// Si no hay temporada activa para esa fecha, Temporada = null (precio base aplica).
public sealed class GetTemporadaVigenteUseCase
{
    private readonly ITemporadaRepository _temporadas;

    public GetTemporadaVigenteUseCase(ITemporadaRepository temporadas)
    {
        _temporadas = temporadas;
    }

    public async Task<GetTemporadaVigenteResponse> ExecuteAsync(
        DateTime fecha, CancellationToken ct = default)
    {
        var temporada = await _temporadas.GetByFechaAsync(fecha, ct);

        return new GetTemporadaVigenteResponse(
            temporada is null ? null : temporada.ToDto());
    }
}

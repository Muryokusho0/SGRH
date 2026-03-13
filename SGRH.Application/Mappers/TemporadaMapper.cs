using SGRH.Application.Dtos.Temporadas;
using SGRH.Domain.Entities.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class TemporadaMapper
{
    public static TemporadaDto ToDto(this Temporada temporada) =>
        new(
            TemporadaId: temporada.TemporadaId,
            NombreTemporada: temporada.NombreTemporada,
            FechaInicio: temporada.FechaInicio,
            FechaFin: temporada.FechaFin);

    public static IReadOnlyList<TemporadaDto> ToDtoList(
        this IEnumerable<Temporada> temporadas) =>
        temporadas.Select(ToDto).ToList();
}

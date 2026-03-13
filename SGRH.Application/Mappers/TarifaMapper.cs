using SGRH.Application.Dtos.Tarifas;
using SGRH.Domain.Entities.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class TarifaMapper
{
    // Requiere NombreCategoria y NombreTemporada porque TarifaTemporada
    // no tiene navegaciones — el UseCase los carga y los pasa aquí.
    public static TarifaTemporadaDto ToDto(
        this TarifaTemporada tarifa,
        string nombreCategoria,
        string nombreTemporada) =>
        new(
            TarifaTemporadaId: tarifa.TarifaTemporadaId,
            CategoriaHabitacionId: tarifa.CategoriaHabitacionId,
            NombreCategoria: nombreCategoria,
            TemporadaId: tarifa.TemporadaId,
            NombreTemporada: nombreTemporada,
            Precio: tarifa.Precio);

    public static IReadOnlyList<TarifaTemporadaDto> ToDtoList(
        this IEnumerable<TarifaTemporada> tarifas,
        IReadOnlyDictionary<int, string> nombresPorCategoria,
        IReadOnlyDictionary<int, string> nombresPorTemporada) =>
        tarifas.Select(t => t.ToDto(
            nombresPorCategoria.TryGetValue(t.CategoriaHabitacionId, out var nc) ? nc : string.Empty,
            nombresPorTemporada.TryGetValue(t.TemporadaId, out var nt) ? nt : string.Empty))
        .ToList();
}

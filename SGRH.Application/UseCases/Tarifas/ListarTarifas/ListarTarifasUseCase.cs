using SGRH.Application.Dtos.Tarifas;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Tarifas.ListarTarifas;

public sealed class ListarTarifasUseCase
{
    private readonly ITarifaTemporadaRepository _tarifas;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly ITemporadaRepository _temporadas;

    public ListarTarifasUseCase(
        ITarifaTemporadaRepository tarifas,
        ICategoriaHabitacionRepository categorias,
        ITemporadaRepository temporadas)
    {
        _tarifas = tarifas;
        _categorias = categorias;
        _temporadas = temporadas;
    }

    public async Task<ListarTarifasResponse> ExecuteAsync(
        int? categoriaId = null,
        int? temporadaId = null,
        CancellationToken ct = default)
    {
        var tarifas = await _tarifas.BuscarAsync(categoriaId, temporadaId, ct);

        if (tarifas.Count == 0)
            return new ListarTarifasResponse([]);

        // Lookups en batch
        var categoriaIds = tarifas.Select(t => t.CategoriaHabitacionId).Distinct().ToList();
        var temporadaIds = tarifas.Select(t => t.TemporadaId).Distinct().ToList();

        var nombresPorCategoria = new Dictionary<int, string>();
        foreach (var cid in categoriaIds)
        {
            var cat = await _categorias.GetByIdAsync(cid, ct);
            if (cat is not null)
                nombresPorCategoria[cid] = cat.NombreCategoria;
        }

        var nombresPorTemporada = new Dictionary<int, string>();
        foreach (var tid in temporadaIds)
        {
            var temp = await _temporadas.GetByIdAsync(tid, ct);
            if (temp is not null)
                nombresPorTemporada[tid] = temp.NombreTemporada;
        }

        var dtos = tarifas
            .ToDtoList(nombresPorCategoria, nombresPorTemporada)
            .ToList();

        return new ListarTarifasResponse(dtos);
    }
}
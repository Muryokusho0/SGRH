using SGRH.Application.Dtos.Habitaciones;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Habitaciones.ListarHabitaciones;

public sealed class ListarHabitacionesUseCase
{
    private readonly IHabitacionRepository _habitaciones;
    private readonly ICategoriaHabitacionRepository _categorias;

    public ListarHabitacionesUseCase(
        IHabitacionRepository habitaciones,
        ICategoriaHabitacionRepository categorias)
    {
        _habitaciones = habitaciones;
        _categorias = categorias;
    }

    public async Task<ListarHabitacionesResponse> ExecuteAsync(
        string? estado = null,
        int? categoriaId = null,
        int? piso = null,
        CancellationToken ct = default)
    {
        var habitaciones = await _habitaciones.BuscarAsync(estado, categoriaId, piso, ct);

        if (habitaciones.Count == 0)
            return new ListarHabitacionesResponse([]);

        // Lookup en batch: IDs únicos de categoría presentes en el resultado
        var categoriaIds = habitaciones
            .Select(h => h.CategoriaHabitacionId)
            .Distinct()
            .ToList();

        var nombresPorCategoria = new Dictionary<int, string>();
        foreach (var cid in categoriaIds)
        {
            var cat = await _categorias.GetByIdAsync(cid, ct);
            if (cat is not null)
                nombresPorCategoria[cid] = cat.NombreCategoria;
        }

        var dtos = habitaciones
            .ToDtoList(nombresPorCategoria)
            .ToList();

        return new ListarHabitacionesResponse(dtos);
    }
}
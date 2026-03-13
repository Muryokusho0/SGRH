using SGRH.Application.Dtos.Categorias;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Categorias.ListarCategorias;

public sealed class ListarCategoriasUseCase
{
    private readonly ICategoriaHabitacionRepository _categorias;

    public ListarCategoriasUseCase(ICategoriaHabitacionRepository categorias)
    {
        _categorias = categorias;
    }

    public async Task<ListarCategoriasResponse> ExecuteAsync(
        string? nombre = null,
        int? capacidadMinima = null,
        int? capacidadMaxima = null,
        CancellationToken ct = default)
    {
        var categorias = await _categorias.BuscarAsync(
            nombre, capacidadMinima, capacidadMaxima, ct);

        var dtos = categorias.Select(c => c.ToDto()).ToList();

        return new ListarCategoriasResponse(dtos);
    }
}
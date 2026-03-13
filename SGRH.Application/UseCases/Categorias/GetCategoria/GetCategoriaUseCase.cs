using SGRH.Domain.Abstractions.Repositories;
using SGRH.Application.Mappers;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Categorias.GetCategoria;

public sealed class GetCategoriaUseCase
{
    private readonly ICategoriaHabitacionRepository _categorias;

    public GetCategoriaUseCase(ICategoriaHabitacionRepository categorias)
    {
        _categorias = categorias;
    }

    public async Task<GetCategoriaResponse> ExecuteAsync(
        int categoriaId, CancellationToken ct = default)
    {
        var categoria = await _categorias.GetByIdAsync(categoriaId, ct)
            ?? throw new NotFoundException("CategoriaHabitacion", categoriaId.ToString());

        return new GetCategoriaResponse(categoria.ToDto());
    }
}

using SGRH.Application.Dtos.Categorias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Categorias.ListarCategorias;

public sealed record ListarCategoriasResponse(IReadOnlyList<CategoriaDto> Categorias);

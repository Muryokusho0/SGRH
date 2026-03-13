using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Application.Dtos.Categorias;

namespace SGRH.Application.UseCases.Categorias.ModificarCategoria;

public sealed record ModificarCategoriaResponse(CategoriaDto Categoria);

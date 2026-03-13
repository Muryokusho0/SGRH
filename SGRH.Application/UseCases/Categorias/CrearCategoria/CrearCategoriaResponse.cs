using SGRH.Application.Dtos.Categorias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Categorias.CrearCategoria;

public sealed record CrearCategoriaResponse(CategoriaDto Categoria);

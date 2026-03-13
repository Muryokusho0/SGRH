using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Tarifas;

public sealed record TarifaTemporadaDto(
    int TarifaTemporadaId,
    int CategoriaHabitacionId,
    string NombreCategoria,
    int TemporadaId,
    string NombreTemporada,
    decimal Precio);


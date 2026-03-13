using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Temporadas;

public sealed record TemporadaDto(
    int TemporadaId,
    string NombreTemporada,
    DateTime FechaInicio,
    DateTime FechaFin);


using SGRH.Application.Dtos.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.CrearTemporada;

public sealed record CrearTemporadaResponse(TemporadaDto Temporada);

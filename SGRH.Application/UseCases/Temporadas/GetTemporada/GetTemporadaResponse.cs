using SGRH.Application.Dtos.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.GetTemporada;

public sealed record GetTemporadaResponse(TemporadaDto Temporada);

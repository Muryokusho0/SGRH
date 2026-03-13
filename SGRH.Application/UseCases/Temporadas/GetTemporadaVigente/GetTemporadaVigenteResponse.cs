using SGRH.Application.Dtos.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.GetTemporadaVigente;

// Puede ser null si la fecha consultada no cae en ninguna temporada definida.
public sealed record GetTemporadaVigenteResponse(TemporadaDto? Temporada);
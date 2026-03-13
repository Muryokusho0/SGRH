using SGRH.Application.Dtos.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.ListarTemporadas;

public sealed record ListarTemporadasResponse(IReadOnlyList<TemporadaDto> Temporadas);

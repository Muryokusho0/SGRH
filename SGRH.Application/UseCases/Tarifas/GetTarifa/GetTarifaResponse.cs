using SGRH.Application.Dtos.Tarifas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Tarifas.GetTarifa;

public sealed record GetTarifaResponse(TarifaTemporadaDto Tarifa);

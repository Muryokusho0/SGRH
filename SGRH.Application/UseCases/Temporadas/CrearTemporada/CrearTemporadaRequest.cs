using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.CrearTemporada;

public sealed record CrearTemporadaRequest(
    string NombreTemporada,
    DateTime FechaInicio,
    DateTime FechaFin,
    AuditInfo AuditInfo);

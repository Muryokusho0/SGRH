using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Tarifas.CrearTarifa;

public sealed record CrearTarifaRequest(
    int CategoriaHabitacionId,
    int TemporadaId,
    decimal PrecioNoche,
    AuditInfo AuditInfo);

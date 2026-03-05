using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Queries.Models;

public sealed class ReportesConfiguration
{
    public DateTime? Desde { get; init; }
    public DateTime? Hasta { get; init; }

    public int? CategoriaHabitacionId { get; init; }
    public int? HabitacionId { get; init; }
    public int? ServicioAdicionalId { get; init; }

    /// Si se especifica, filtra por reservas de un cliente.
    public int? ClienteId { get; init; }

    /// Límite opcional para rankings (top servicios, etc.)
    public int? Top { get; init; }
}
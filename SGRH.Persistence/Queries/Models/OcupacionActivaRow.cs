using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Queries.Models;

public sealed class OcupacionActivaRow
{
    public int ReservaId { get; init; }

    public int HabitacionId { get; init; }
    public string? HabitacionCodigo { get; init; }

    public int CategoriaHabitacionId { get; init; }
    public string? CategoriaNombre { get; init; }

    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }

    public string? EstadoReserva { get; init; }
    public string? ClienteNombre { get; init; }
}

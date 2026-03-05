using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Queries.Models;

public sealed class ReservaCostoTotalRow
{
    public int ReservaId { get; init; }
    public int ClienteId { get; init; }
    public string? ClienteNombre { get; init; }

    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }

    public decimal TotalHabitaciones { get; init; }
    public decimal TotalServicios { get; init; }
    public decimal TotalReserva => TotalHabitaciones + TotalServicios;
}

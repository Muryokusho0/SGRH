using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Reservas;

public sealed record DetalleReservaDto(
    int HabitacionId,
    int NumeroHabitacion,
    string NombreCategoria,
    decimal TarifaAplicada);

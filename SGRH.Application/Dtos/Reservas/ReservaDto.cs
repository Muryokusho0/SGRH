using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Reservas;

public sealed record ReservaDto(
    int ReservaId,
    int ClienteId,
    string NombreCliente,
    string EstadoReserva,
    DateTime FechaReserva,
    DateTime FechaEntrada,
    DateTime FechaSalida,
    decimal CostoTotal,
    IReadOnlyList<DetalleReservaDto> Habitaciones,
    IReadOnlyList<ReservaServicioDto> Servicios);
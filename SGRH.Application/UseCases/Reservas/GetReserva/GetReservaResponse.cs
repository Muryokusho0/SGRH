using SGRH.Application.Dtos.Reservas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.GetReserva;

public sealed record GetReservaResponse(ReservaDto Reserva);

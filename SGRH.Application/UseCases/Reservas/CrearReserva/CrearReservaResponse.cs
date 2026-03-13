using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.CrearReserva;

public sealed record CrearReservaResponse(int ReservaId, DateTime FechaReserva);
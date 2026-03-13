using SGRH.Application.Dtos.Reservas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.ListarReservas;

public sealed record ListarReservasResponse(IReadOnlyList<ReservaDto> Reservas);

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Reservas;

public sealed record ReservaServicioDto(
    int ServicioAdicionalId,
    string NombreServicio,
    string TipoServicio,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);
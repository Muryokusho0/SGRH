using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRH.Persistence.Context;

namespace SGRH.Persistence.Queries.Models;

public sealed class UsoServiciosRow
{
    public int ServicioAdicionalId { get; init; }
    public string? ServicioNombre { get; init; }

    public int CantidadSolicitudes { get; init; }

    /// Monto total cobrado por el servicio en el rango de fechas.
    public decimal IngresoTotal { get; init; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Servicios;

public sealed record ServicioDto(
    int ServicioAdicionalId,
    string NombreServicio,
    string TipoServicio,
    IReadOnlyList<int> TemporadaIds);

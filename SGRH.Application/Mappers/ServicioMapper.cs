using SGRH.Application.Dtos.Servicios;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class ServicioMapper
{
    public static ServicioDto ToDto(this ServicioAdicional servicio) =>
        new(
            ServicioAdicionalId: servicio.ServicioAdicionalId,
            NombreServicio: servicio.NombreServicio,
            TipoServicio: servicio.TipoServicio,
            TemporadaIds: servicio.TemporadaIds.ToList());

    public static IReadOnlyList<ServicioDto> ToDtoList(
        this IEnumerable<ServicioAdicional> servicios) =>
        servicios.Select(ToDto).ToList();
}


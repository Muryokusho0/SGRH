using SGRH.Application.Dtos.Auditoria;
using SGRH.Domain.Entities.Auditoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class AuditoriaMapper
{
    public static AuditoriaEventoDetalleDto ToDto(
        this AuditoriaEventoDetalle detalle) =>
        new(
            Campo: detalle.Campo,
            ValorAnterior: detalle.ValorAnterior,
            ValorNuevo: detalle.ValorNuevo);

    public static AuditoriaEventoDto ToDto(this AuditoriaEvento evento) =>
        new(
            AuditoriaEventoId: evento.AuditoriaEventoId,
            FechaUtc: evento.FechaUtc,
            UsuarioId: evento.UsuarioId,
            UsernameSnapshot: evento.UsernameSnapshot,
            Rol: evento.Rol,
            Accion: evento.Accion,
            Modulo: evento.Modulo,
            Entidad: evento.Entidad,
            EntidadId: evento.EntidadId,
            RequestId: evento.RequestId,
            IpOrigen: evento.IpOrigen,
            UserAgent: evento.UserAgent,
            Descripcion: evento.Descripcion,
            Detalles: evento.Detalles.Select(d => d.ToDto()).ToList());

    public static IReadOnlyList<AuditoriaEventoDto> ToDtoList(
        this IEnumerable<AuditoriaEvento> eventos) =>
        eventos.Select(ToDto).ToList();
}

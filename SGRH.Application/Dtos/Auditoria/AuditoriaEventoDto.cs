using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Auditoria;

public sealed record AuditoriaEventoDetalleDto(
    string Campo,
    string? ValorAnterior,
    string? ValorNuevo);

public sealed record AuditoriaEventoDto(
    long AuditoriaEventoId,
    DateTime FechaUtc,
    int UsuarioId,
    string UsernameSnapshot,
    string Rol,
    string Accion,
    string Modulo,
    string Entidad,
    string EntidadId,
    Guid RequestId,
    string IpOrigen,
    string UserAgent,
    string Descripcion,
    IReadOnlyList<AuditoriaEventoDetalleDto> Detalles);

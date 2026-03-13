using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.QuitarServicio;

public sealed class QuitarServicioUseCase
{
    private readonly IReservaRepository _reservas;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public QuitarServicioUseCase(
        IReservaRepository reservas,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _reservas = reservas;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(
        QuitarServicioRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        var reserva = await _reservas.GetByIdWithDetallesAsync(request.ReservaId, ct)
            ?? throw new NotFoundException("Reserva", request.ReservaId.ToString());

        reserva.QuitarServicio(request.ServicioAdicionalId);

        await _uow.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(new AuditoriaEvento(
            usuarioId: usuarioActualId,
            rol: usuarioActualRol,
            usernameSnapshot: usernameActual,
            accion: "UPDATE",
            modulo: "Reservas",
            entidad: "Reserva",
            entidadId: reserva.ReservaId.ToString(),
            requestId: request.AuditInfo.RequestId,
            ipOrigen: request.AuditInfo.IpOrigen,
            userAgent: request.AuditInfo.UserAgent,
            descripcion: $"Servicio {request.ServicioAdicionalId} removido de reserva {reserva.ReservaId}"), ct);
    }
}

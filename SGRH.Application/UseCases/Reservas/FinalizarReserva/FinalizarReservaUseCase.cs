using SGRH.Application.Abstractions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.FinalizarReserva;

public sealed class FinalizarReservaUseCase
{
    private readonly IReservaRepository _reservas;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public FinalizarReservaUseCase(
        IReservaRepository reservas,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _reservas = reservas;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(
        int reservaId,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        AuditInfo auditInfo,
        CancellationToken ct = default)
    {
        var reserva = await _reservas.GetByIdWithDetallesAsync(reservaId, ct)
            ?? throw new NotFoundException("Reserva", reservaId.ToString());

        reserva.Finalizar();

        await _uow.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(new AuditoriaEvento(
            usuarioId: usuarioActualId,
            rol: usuarioActualRol,
            usernameSnapshot: usernameActual,
            accion: "UPDATE",
            modulo: "Reservas",
            entidad: "Reserva",
            entidadId: reserva.ReservaId.ToString(),
            requestId: auditInfo.RequestId,
            ipOrigen: auditInfo.IpOrigen,
            userAgent: auditInfo.UserAgent,
            descripcion: $"Reserva {reserva.ReservaId} finalizada. CostoTotal: {reserva.CostoTotal:C}"), ct);
    }
}

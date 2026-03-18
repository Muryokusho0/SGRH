using SGRH.Application.Abstractions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Reservas.CancelarReserva;

public sealed class CancelarReservaUseCase
{
    private readonly IReservaRepository _reservas;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public CancelarReservaUseCase(
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
        // ── 1. Buscar — lectura fuera de transacción ──────────────────────
        var reserva = await _reservas.GetByIdWithDetallesAsync(reservaId, ct)
            ?? throw new NotFoundException("Reserva", reservaId.ToString());

        // ── 2. Transacción ────────────────────────────────────────────────
        await _uow.BeginTransactionAsync(ct);
        try
        {
            // La entidad lanza BusinessRuleViolationException si ya está cancelada o finalizada
            reserva.Cancelar();

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
                descripcion: $"Reserva {reserva.ReservaId} cancelada."), ct);

            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
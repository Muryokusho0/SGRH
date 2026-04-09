using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Reservas.AgregarHabitacion;

public sealed class AgregarHabitacionUseCase
{
    private readonly IValidator<AgregarHabitacionRequest> _validator;
    private readonly IReservaRepository _reservas;
    private readonly IReservaDomainPolicy _policy;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public AgregarHabitacionUseCase(
        IValidator<AgregarHabitacionRequest> validator,
        IReservaRepository reservas,
        IReservaDomainPolicy policy,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _validator = validator;
        _reservas = reservas;
        _policy = policy;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(
        AgregarHabitacionRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        var validacion = await _validator.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            throw new ApplicationValidationException(validacion.Errors);

        var reserva = await _reservas.GetByIdWithDetallesAsync(request.ReservaId, ct)
            ?? throw new NotFoundException("Reserva", request.ReservaId.ToString());

        await _uow.BeginTransactionAsync(ct);
        try
        {
            reserva.AgregarHabitacion(request.HabitacionId, _policy);

            await _uow.SaveChangesAsync(ct);

            var evento = new AuditoriaEvento(
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
                descripcion: $"Habitación {request.HabitacionId} agregada a reserva {reserva.ReservaId}.");

            // ── Detalle del cambio ────────────────────────────────────────
            evento.AgregarDetalle(
                campo: "HabitacionId",
                valorAnterior: null,
                valorNuevo: request.HabitacionId.ToString());

            await _auditoria.RegistrarAsync(evento, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Habitaciones.BloquearHabitacion;

public sealed class BloquearHabitacionUseCase
{
    private readonly IHabitacionRepository _habitaciones;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<BloquearHabitacionRequest> _validator;

    public BloquearHabitacionUseCase(
        IHabitacionRepository habitaciones,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<BloquearHabitacionRequest> validator)
    {
        _habitaciones = habitaciones;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task ExecuteAsync(
        BloquearHabitacionRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar — fuera de transacción ─────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Buscar CON historial — fuera de transacción ────────────────
        var habitacion = await _habitaciones.GetByIdWithHistorialAsync(request.HabitacionId, ct)
            ?? throw new NotFoundException("Habitacion", request.HabitacionId.ToString());

        // ── 3. Snapshot del estado anterior para auditoría ────────────────
        var estadoAnterior = habitacion.EstadoActual?.EstadoHabitacion.ToString() ?? "Desconocido";

        // ── 4. Bloquear (entidad valida reglas de motivo internamente) ────
        habitacion.CambiarEstado(EstadoHabitacion.Mantenimiento, request.Motivo);

        // ── 5. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);

            var evento = new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "BLOCK",
                modulo: "Habitaciones",
                entidad: "Habitacion",
                entidadId: habitacion.HabitacionId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Habitación {habitacion.NumeroHabitacion} bloqueada. Motivo: {request.Motivo}.");

            evento.AgregarDetalle("EstadoHabitacion", estadoAnterior, nameof(EstadoHabitacion.Mantenimiento));
            evento.AgregarDetalle("MotivoCambio", null, request.Motivo);

            await _auditoria.RegistrarAsync(evento, ct);

            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
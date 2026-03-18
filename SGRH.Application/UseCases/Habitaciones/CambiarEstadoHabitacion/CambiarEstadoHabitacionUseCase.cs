using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Habitaciones.CambiarEstadoHabitacion;

public sealed class CambiarEstadoHabitacionUseCase
{
    private readonly IHabitacionRepository _habitaciones;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<CambiarEstadoHabitacionRequest> _validator;

    public CambiarEstadoHabitacionUseCase(
        IHabitacionRepository habitaciones,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<CambiarEstadoHabitacionRequest> validator)
    {
        _habitaciones = habitaciones;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task ExecuteAsync(
        CambiarEstadoHabitacionRequest request,
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
        var motivoAnterior = habitacion.EstadoActual?.MotivoCambio;

        // ── 4. Cambiar estado ─────────────────────────────────────────────
        var nuevoEstado = Enum.Parse<EstadoHabitacion>(request.NuevoEstado, ignoreCase: true);
        habitacion.CambiarEstado(nuevoEstado, request.Motivo);

        // ── 5. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);

            var descripcion = string.IsNullOrWhiteSpace(request.Motivo)
                ? $"Habitación {habitacion.NumeroHabitacion} cambió a {nuevoEstado}."
                : $"Habitación {habitacion.NumeroHabitacion} cambió a {nuevoEstado}. Motivo: {request.Motivo}.";

            var evento = new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "STATE_CHANGE",
                modulo: "Habitaciones",
                entidad: "Habitacion",
                entidadId: habitacion.HabitacionId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: descripcion);

            // Siempre hay cambio de estado — registrar el campo
            evento.AgregarDetalle("EstadoHabitacion", estadoAnterior, nuevoEstado.ToString());

            // Registrar motivo solo si cambió (puede pasar de con motivo a sin motivo)
            if (!string.Equals(motivoAnterior, request.Motivo, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("MotivoCambio", motivoAnterior, request.Motivo);

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
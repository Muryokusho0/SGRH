using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using SGRH.Application.Abstractions;
using SGRH.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Buscar ─────────────────────────────────────────────────────
        var habitacion = await _habitaciones.GetByIdAsync(request.HabitacionId, ct)
            ?? throw new NotFoundException("Habitacion", request.HabitacionId.ToString());

        // ── 3. Bloquear — la entidad gestiona el historial internamente ───
        // CambiarEstado lanza BusinessRuleViolationException si ya está en mantenimiento
        habitacion.CambiarEstado(EstadoHabitacion.Mantenimiento, request.Motivo);

        await _unitOfWork.SaveChangesAsync(ct);

        // ── 4. Auditoría ──────────────────────────────────────────────────
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
            descripcion: $"Habitación {habitacion.NumeroHabitacion} bloqueada por mantenimiento. Motivo: {request.Motivo}.");

        await _auditoria.RegistrarAsync(evento, ct);
    }
}
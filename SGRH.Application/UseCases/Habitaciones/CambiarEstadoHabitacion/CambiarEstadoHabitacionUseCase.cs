using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Buscar ─────────────────────────────────────────────────────
        var habitacion = await _habitaciones.GetByIdAsync(request.HabitacionId, ct)
            ?? throw new NotFoundException("Habitacion", request.HabitacionId.ToString());

        // ── 3. Parsear estado — el Validator garantiza que es válido ──────
        var nuevoEstado = Enum.Parse<EstadoHabitacion>(
            request.NuevoEstado, ignoreCase: true);

        // ── 4. Cambiar estado — la entidad lanza si ya está en ese estado ─
        habitacion.CambiarEstado(nuevoEstado, request.Motivo);

        await _unitOfWork.SaveChangesAsync(ct);

        // ── 5. Auditoría ──────────────────────────────────────────────────
        var descripcion = string.IsNullOrWhiteSpace(request.Motivo)
            ? $"Habitación {habitacion.NumeroHabitacion} cambió a estado {nuevoEstado}."
            : $"Habitación {habitacion.NumeroHabitacion} cambió a estado {nuevoEstado}. Motivo: {request.Motivo}.";

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

        await _auditoria.RegistrarAsync(evento, ct);
    }
}
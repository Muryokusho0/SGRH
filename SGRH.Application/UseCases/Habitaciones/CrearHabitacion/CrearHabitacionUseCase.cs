using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Exceptions;
using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.CrearHabitacion;

public sealed class CrearHabitacionUseCase
{
    private readonly IHabitacionRepository _habitaciones;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<CrearHabitacionRequest> _validator;

    public CrearHabitacionUseCase(
        IHabitacionRepository habitaciones,
        ICategoriaHabitacionRepository categorias,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<CrearHabitacionRequest> validator)
    {
        _habitaciones = habitaciones;
        _categorias = categorias;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<CrearHabitacionResponse> ExecuteAsync(
        CrearHabitacionRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Verificar que la categoría existe ──────────────────────────
        var categoria = await _categorias.GetByIdAsync(request.CategoriaHabitacionId, ct)
            ?? throw new NotFoundException(
                "CategoriaHabitacion", request.CategoriaHabitacionId.ToString());

        // ── 3. Número de habitación único ─────────────────────────────────
        if (await _habitaciones.ExistsByNumeroAsync(request.NumeroHabitacion, ct))
            throw new ConflictException(
                $"Ya existe una habitación con el número '{request.NumeroHabitacion}'.");

        // ── 4. Crear — orden exacto del constructor de la entidad ─────────
        var habitacion = new Habitacion(
            request.CategoriaHabitacionId,
            request.NumeroHabitacion,
            request.NumeroPiso);

        await _habitaciones.AddAsync(habitacion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // ── 5. Auditoría ──────────────────────────────────────────────────
        var evento = new AuditoriaEvento(
            usuarioId: usuarioActualId,
            rol: usuarioActualRol,
            usernameSnapshot: usernameActual,
            accion: "CREATE",
            modulo: "Habitaciones",
            entidad: "Habitacion",
            entidadId: habitacion.HabitacionId.ToString(),
            requestId: request.AuditInfo.RequestId,
            ipOrigen: request.AuditInfo.IpOrigen,
            userAgent: request.AuditInfo.UserAgent,
            descripcion: $"Habitación {request.NumeroHabitacion} creada en piso {request.NumeroPiso}, categoría '{categoria.NombreCategoria}'.");

        await _auditoria.RegistrarAsync(evento, ct);

        return new CrearHabitacionResponse(
            HabitacionMapper.ToDto(habitacion, categoria.NombreCategoria));
    }
}
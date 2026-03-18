using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Exceptions;

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
        // ── 1. Validar — fuera de transacción ─────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Lecturas de verificación — fuera de transacción ────────────
        var categoria = await _categorias.GetByIdAsync(request.CategoriaHabitacionId, ct)
            ?? throw new NotFoundException(
                "CategoriaHabitacion", request.CategoriaHabitacionId.ToString());

        if (await _habitaciones.ExistsByNumeroAsync(request.NumeroHabitacion, ct))
            throw new ConflictException(
                $"Ya existe una habitación con el número '{request.NumeroHabitacion}'.");

        // ── 3. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var habitacion = new Habitacion(
                request.CategoriaHabitacionId,
                request.NumeroHabitacion,
                request.NumeroPiso);

            await _habitaciones.AddAsync(habitacion, ct);
            await _unitOfWork.SaveChangesAsync(ct); // flush para obtener el ID generado

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
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
                descripcion: $"Habitación {request.NumeroHabitacion} creada en piso {request.NumeroPiso}, categoría '{categoria.NombreCategoria}'."), ct);

            await _unitOfWork.CommitAsync(ct);
            return new CrearHabitacionResponse(
                HabitacionMapper.ToDto(habitacion, categoria.NombreCategoria));
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
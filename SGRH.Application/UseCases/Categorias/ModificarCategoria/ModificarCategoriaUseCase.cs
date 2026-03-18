using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Categorias.ModificarCategoria;

public sealed class ModificarCategoriaUseCase
{
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<ModificarCategoriaRequest> _validator;

    public ModificarCategoriaUseCase(
        ICategoriaHabitacionRepository categorias,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<ModificarCategoriaRequest> validator)
    {
        _categorias = categorias;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<ModificarCategoriaResponse> ExecuteAsync(
        ModificarCategoriaRequest request,
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

        if (!string.Equals(categoria.NombreCategoria, request.NombreCategoria,
                StringComparison.OrdinalIgnoreCase))
        {
            if (await _categorias.ExistsByNombreAsync(request.NombreCategoria, ct))
                throw new ConflictException(
                    $"Ya existe una categoría con el nombre '{request.NombreCategoria}'.");
        }

        // ── 3. Snapshots ANTES de modificar (para el detalle de auditoría) ─
        var nombreAnterior = categoria.NombreCategoria;
        var capacidadAnterior = categoria.Capacidad;
        var descripAnterior = categoria.Descripcion;
        var precioAnterior = categoria.PrecioBase;

        // ── 4. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            categoria.Actualizar(
                request.NombreCategoria,
                request.Capacidad,
                request.Descripcion,
                request.PrecioBase);

            await _unitOfWork.SaveChangesAsync(ct);

            // ── 5. Auditoría con detalle campo por campo ──────────────────
            var evento = new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "UPDATE",
                modulo: "Categorias",
                entidad: "CategoriaHabitacion",
                entidadId: categoria.CategoriaHabitacionId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Categoría {categoria.CategoriaHabitacionId} modificada.");

            if (!string.Equals(nombreAnterior, request.NombreCategoria, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("NombreCategoria", nombreAnterior, request.NombreCategoria);

            if (capacidadAnterior != request.Capacidad)
                evento.AgregarDetalle("Capacidad",
                    capacidadAnterior.ToString(),
                    request.Capacidad.ToString());

            if (!string.Equals(descripAnterior, request.Descripcion, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("Descripcion", descripAnterior, request.Descripcion);

            if (precioAnterior != request.PrecioBase)
                evento.AgregarDetalle("PrecioBase",
                    precioAnterior.ToString("F2"),
                    request.PrecioBase.ToString("F2"));

            await _auditoria.RegistrarAsync(evento, ct);

            await _unitOfWork.CommitAsync(ct);
            return new ModificarCategoriaResponse(categoria.ToDto());
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
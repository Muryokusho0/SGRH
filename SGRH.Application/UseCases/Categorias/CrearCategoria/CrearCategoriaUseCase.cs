using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Exceptions;
using SGRH.Application.Abstractions;
using SGRH.Application.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Categorias.CrearCategoria;

public sealed class CrearCategoriaUseCase
{
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<CrearCategoriaRequest> _validator;

    public CrearCategoriaUseCase(
        ICategoriaHabitacionRepository categorias,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<CrearCategoriaRequest> validator)
    {
        _categorias = categorias;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<CrearCategoriaResponse> ExecuteAsync(
        CrearCategoriaRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Unicidad de nombre ─────────────────────────────────────────
        if (await _categorias.ExistsByNombreAsync(request.NombreCategoria, ct))
            throw new ConflictException(
                $"Ya existe una categoría con el nombre '{request.NombreCategoria}'.");

        // ── 3. Crear ──────────────────────────────────────────────────────
        var categoria = new CategoriaHabitacion(
            request.NombreCategoria,
            request.Capacidad,
            request.Descripcion,
            request.PrecioBase);

        await _categorias.AddAsync(categoria, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // ── 4. Auditoría ──────────────────────────────────────────────────
        var evento = new AuditoriaEvento(
            usuarioId: usuarioActualId,
            rol: usuarioActualRol,
            usernameSnapshot: usernameActual,
            accion: "CREATE",
            modulo: "Categorias",
            entidad: "CategoriaHabitacion",
            entidadId: categoria.CategoriaHabitacionId.ToString(),
            requestId: request.AuditInfo.RequestId,
            ipOrigen: request.AuditInfo.IpOrigen,
            userAgent: request.AuditInfo.UserAgent,
            descripcion: $"Categoría '{request.NombreCategoria}' creada.");

        await _auditoria.RegistrarAsync(evento, ct);

        return new CrearCategoriaResponse(categoria.ToDto());
    }
}

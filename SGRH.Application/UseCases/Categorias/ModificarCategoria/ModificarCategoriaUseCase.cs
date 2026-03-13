using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Application.Abstractions;
using SGRH.Application.Mappers;

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
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Buscar ─────────────────────────────────────────────────────
        var categoria = await _categorias.GetByIdAsync(request.CategoriaHabitacionId, ct)
            ?? throw new NotFoundException(
                "CategoriaHabitacion", request.CategoriaHabitacionId.ToString());

        // ── 3. Unicidad de nombre (si cambió) ─────────────────────────────
        if (!string.Equals(categoria.NombreCategoria, request.NombreCategoria,
                StringComparison.OrdinalIgnoreCase))
        {
            if (await _categorias.ExistsByNombreAsync(request.NombreCategoria, ct))
                throw new ConflictException(
                    $"Ya existe una categoría con el nombre '{request.NombreCategoria}'.");
        }

        // ── 4. Modificar ──────────────────────────────────────────────────
        categoria.Actualizar(
            request.NombreCategoria,
            request.Capacidad,
            request.Descripcion,
            request.PrecioBase);

        await _unitOfWork.SaveChangesAsync(ct);

        // ── 5. Auditoría ──────────────────────────────────────────────────
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

        await _auditoria.RegistrarAsync(evento, ct);

        return new ModificarCategoriaResponse(categoria.ToDto());
    }
}
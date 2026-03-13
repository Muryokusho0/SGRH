using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Temporadas;
using SGRH.Domain.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.CrearTemporada;

public sealed class CrearTemporadaUseCase
{
    private readonly ITemporadaRepository _temporadas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<CrearTemporadaRequest> _validator;

    public CrearTemporadaUseCase(
        ITemporadaRepository temporadas,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<CrearTemporadaRequest> validator)
    {
        _temporadas = temporadas;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<CrearTemporadaResponse> ExecuteAsync(
        CrearTemporadaRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. RF-09: No superposición de temporadas ───────────────────────
        if (await _temporadas.ExisteSolapamientoAsync(
                request.FechaInicio, request.FechaFin, excludeId: null, ct))
            throw new ConflictException(
                "El rango de fechas se superpone con una temporada existente.");

        // ── 3. Crear ──────────────────────────────────────────────────────
        var temporada = new Temporada(
            request.NombreTemporada,
            request.FechaInicio,
            request.FechaFin);

        await _temporadas.AddAsync(temporada, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // ── 4. Auditoría ──────────────────────────────────────────────────
        var evento = new AuditoriaEvento(
            usuarioId: usuarioActualId,
            rol: usuarioActualRol,
            usernameSnapshot: usernameActual,
            accion: "CREATE",
            modulo: "Temporadas",
            entidad: "Temporada",
            entidadId: temporada.TemporadaId.ToString(),
            requestId: request.AuditInfo.RequestId,
            ipOrigen: request.AuditInfo.IpOrigen,
            userAgent: request.AuditInfo.UserAgent,
            descripcion: $"Temporada '{request.NombreTemporada}' creada ({request.FechaInicio:d} – {request.FechaFin:d}).");

        await _auditoria.RegistrarAsync(evento, ct);

        return new CrearTemporadaResponse(temporada.ToDto());
    }
}

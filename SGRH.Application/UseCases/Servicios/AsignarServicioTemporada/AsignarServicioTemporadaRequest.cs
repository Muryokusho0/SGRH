using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Servicios.AsignarServicioTemporada;

// ── Request ───────────────────────────────────────────────────────────────
public sealed record AsignarServicioTemporadaRequest(
    int ServicioAdicionalId,
    int TemporadaId,
    AuditInfo AuditInfo);

// ── Response ──────────────────────────────────────────────────────────────
public sealed record AsignarServicioTemporadaResponse(
    int ServicioAdicionalId,
    string NombreServicio,
    int TemporadaId,
    string NombreTemporada);

// ── Validator ─────────────────────────────────────────────────────────────
public sealed class AsignarServicioTemporadaValidator
    : IValidator<AsignarServicioTemporadaRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        AsignarServicioTemporadaRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.ServicioAdicionalId <= 0)
            errors.Add("El ServicioAdicionalId no es válido.");

        if (request.TemporadaId <= 0)
            errors.Add("El TemporadaId no es válido.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}

// ── UseCase ───────────────────────────────────────────────────────────────
public sealed class AsignarServicioTemporadaUseCase
{
    private readonly IServicioAdicionalRepository _servicios;
    private readonly ITemporadaRepository _temporadas;
    private readonly IServicioTemporadaRepository _servicioTemporadas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<AsignarServicioTemporadaRequest> _validator;

    public AsignarServicioTemporadaUseCase(
        IServicioAdicionalRepository servicios,
        ITemporadaRepository temporadas,
        IServicioTemporadaRepository servicioTemporadas,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<AsignarServicioTemporadaRequest> validator)
    {
        _servicios = servicios;
        _temporadas = temporadas;
        _servicioTemporadas = servicioTemporadas;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<AsignarServicioTemporadaResponse> ExecuteAsync(
        AsignarServicioTemporadaRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Verificar que existen servicio y temporada ─────────────────
        var servicio = await _servicios.GetByIdAsync(request.ServicioAdicionalId, ct)
            ?? throw new NotFoundException("ServicioAdicional", request.ServicioAdicionalId.ToString());

        var temporada = await _temporadas.GetByIdAsync(request.TemporadaId, ct)
            ?? throw new NotFoundException("Temporada", request.TemporadaId.ToString());

        // ── 3. Verificar que no existe ya ─────────────────────────────────
        if (await _servicioTemporadas.ExisteAsync(request.ServicioAdicionalId, request.TemporadaId, ct))
            throw new ConflictException(
                $"El servicio '{servicio.NombreServicio}' ya está asignado a la temporada '{temporada.NombreTemporada}'.");

        // ── 4. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var servicioTemporada = new ServicioTemporada(
                request.ServicioAdicionalId,
                request.TemporadaId);

            await _servicioTemporadas.AddAsync(servicioTemporada, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "CREATE",
                modulo: "Servicios",
                entidad: "ServicioTemporada",
                entidadId: $"{request.ServicioAdicionalId}-{request.TemporadaId}",
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Servicio '{servicio.NombreServicio}' asignado a temporada '{temporada.NombreTemporada}'."), ct);

            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }

        return new AsignarServicioTemporadaResponse(
            request.ServicioAdicionalId,
            servicio.NombreServicio,
            request.TemporadaId,
            temporada.NombreTemporada);
    }
}
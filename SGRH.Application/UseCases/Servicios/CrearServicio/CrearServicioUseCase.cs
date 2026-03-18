using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Servicios.CrearServicio;

public sealed class CrearServicioUseCase
{
    private readonly IServicioAdicionalRepository _servicios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<CrearServicioRequest> _validator;

    public CrearServicioUseCase(
        IServicioAdicionalRepository servicios,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<CrearServicioRequest> validator)
    {
        _servicios = servicios;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<CrearServicioResponse> ExecuteAsync(
        CrearServicioRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar — fuera de transacción ─────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Unicidad — lectura fuera de transacción ────────────────────
        if (await _servicios.ExistsByNombreAsync(request.NombreServicio, ct))
            throw new ConflictException(
                $"Ya existe un servicio con el nombre '{request.NombreServicio}'.");

        // ── 3. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var servicio = new ServicioAdicional(
                request.NombreServicio,
                request.Descripcion);

            await _servicios.AddAsync(servicio, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "CREATE",
                modulo: "Servicios",
                entidad: "ServicioAdicional",
                entidadId: servicio.ServicioAdicionalId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Servicio '{request.NombreServicio}' creado."), ct);

            await _unitOfWork.CommitAsync(ct);
            return new CrearServicioResponse(servicio.ToDto());
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
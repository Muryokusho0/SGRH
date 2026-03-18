using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Reservas.CrearReserva;

public sealed class CrearReservaUseCase
{
    private readonly IValidator<CrearReservaRequest> _validator;
    private readonly IClienteRepository _clientes;
    private readonly IReservaRepository _reservas;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public CrearReservaUseCase(
        IValidator<CrearReservaRequest> validator,
        IClienteRepository clientes,
        IReservaRepository reservas,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _validator = validator;
        _clientes = clientes;
        _reservas = reservas;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task<CrearReservaResponse> ExecuteAsync(
        CrearReservaRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar — fuera de transacción ─────────────────────────────
        var validacion = await _validator.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            throw new ApplicationValidationException(validacion.Errors);

        // ── 2. Verificar cliente existe — lectura fuera de transacción ────
        _ = await _clientes.GetByIdAsync(request.ClienteId, ct)
            ?? throw new NotFoundException("Cliente", request.ClienteId.ToString());

        // ── 3. Transacción ────────────────────────────────────────────────
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var reserva = new Reserva(
                request.ClienteId,
                request.FechaEntrada,
                request.FechaSalida);

            await _reservas.AddAsync(reserva, ct);
            await _uow.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "CREATE",
                modulo: "Reservas",
                entidad: "Reserva",
                entidadId: reserva.ReservaId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Reserva creada para cliente {reserva.ClienteId}."), ct);

            await _uow.CommitAsync(ct);
            return new CrearReservaResponse(reserva.ReservaId, reserva.FechaReserva);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
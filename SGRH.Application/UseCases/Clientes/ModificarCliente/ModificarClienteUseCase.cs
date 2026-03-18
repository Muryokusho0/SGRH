using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Clientes.ModificarCliente;

public sealed class ModificarClienteUseCase
{
    private readonly IClienteRepository _clientes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<ModificarClienteRequest> _validator;

    public ModificarClienteUseCase(
        IClienteRepository clientes,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<ModificarClienteRequest> validator)
    {
        _clientes = clientes;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<ModificarClienteResponse> ExecuteAsync(
        ModificarClienteRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar — fuera de transacción ─────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Buscar — fuera de transacción ──────────────────────────────
        var cliente = await _clientes.GetByIdAsync(request.ClienteId, ct)
            ?? throw new NotFoundException("Cliente", request.ClienteId.ToString());

        // ── 3. Unicidad de email (si cambió) — fuera de transacción ───────
        if (!string.Equals(cliente.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existente = await _clientes.GetByEmailAsync(request.Email, ct);
            if (existente is not null && existente.ClienteId != request.ClienteId)
                throw new ConflictException(
                    $"El email '{request.Email}' ya está en uso por otro cliente.");
        }

        // ── 4. Snapshots ANTES de modificar ──────────────────────────────
        var nombreAnterior = cliente.NombreCliente;
        var apellidoAnterior = cliente.ApellidoCliente;
        var emailAnterior = cliente.Email;
        var telefonoAnterior = cliente.Telefono;

        // ── 5. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            cliente.ActualizarDatos(
                request.NombreCliente,
                request.ApellidoCliente,
                request.Email,
                request.Telefono);

            await _unitOfWork.SaveChangesAsync(ct);

            // ── 6. Auditoría con detalle campo por campo ──────────────────
            var evento = new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "UPDATE",
                modulo: "Clientes",
                entidad: "Cliente",
                entidadId: cliente.ClienteId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Datos del cliente {cliente.ClienteId} modificados.");

            if (!string.Equals(nombreAnterior, request.NombreCliente, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("NombreCliente", nombreAnterior, request.NombreCliente);

            if (!string.Equals(apellidoAnterior, request.ApellidoCliente, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("ApellidoCliente", apellidoAnterior, request.ApellidoCliente);

            if (!string.Equals(emailAnterior, request.Email, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("Email", emailAnterior, request.Email);

            if (!string.Equals(telefonoAnterior, request.Telefono, StringComparison.OrdinalIgnoreCase))
                evento.AgregarDetalle("Telefono", telefonoAnterior, request.Telefono);

            await _auditoria.RegistrarAsync(evento, ct);

            await _unitOfWork.CommitAsync(ct);
            return new ModificarClienteResponse(cliente.ToDto());
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
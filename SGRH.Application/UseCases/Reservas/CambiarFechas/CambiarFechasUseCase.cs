using SGRH.Application.Common.Exceptions;
using SGRH.Application.Abstractions;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.CambiarFechas;

public sealed class CambiarFechasUseCase
{
    private readonly IValidator<CambiarFechasRequest> _validator;
    private readonly IReservaRepository _reservas;
    private readonly IReservaDomainPolicy _policy;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public CambiarFechasUseCase(
        IValidator<CambiarFechasRequest> validator,
        IReservaRepository reservas,
        IReservaDomainPolicy policy,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _validator = validator;
        _reservas = reservas;
        _policy = policy;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(
        CambiarFechasRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        var validacion = await _validator.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            throw new ApplicationValidationException(validacion.Errors);

        var reserva = await _reservas.GetByIdWithDetallesAsync(request.ReservaId, ct)
            ?? throw new NotFoundException("Reserva", request.ReservaId.ToString());

        var entradaAnterior = reserva.FechaEntrada;
        var salidaAnterior = reserva.FechaSalida;

        reserva.CambiarFechas(request.NuevaFechaEntrada, request.NuevaFechaSalida, _policy);

        await _uow.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(new AuditoriaEvento(
            usuarioId: usuarioActualId,
            rol: usuarioActualRol,
            usernameSnapshot: usernameActual,
            accion: "UPDATE",
            modulo: "Reservas",
            entidad: "Reserva",
            entidadId: reserva.ReservaId.ToString(),
            requestId: request.AuditInfo.RequestId,
            ipOrigen: request.AuditInfo.IpOrigen,
            userAgent: request.AuditInfo.UserAgent,
            descripcion: $"Fechas cambiadas: {entradaAnterior:d}-{salidaAnterior:d} → {request.NuevaFechaEntrada:d}-{request.NuevaFechaSalida:d}"), ct);
    }
}
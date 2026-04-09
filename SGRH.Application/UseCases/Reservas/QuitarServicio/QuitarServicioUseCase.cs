using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Reservas.QuitarServicio;

public sealed class QuitarServicioUseCase
{
    private readonly IReservaRepository _reservas;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public QuitarServicioUseCase(
        IReservaRepository reservas,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _reservas = reservas;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(
        QuitarServicioRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        var reserva = await _reservas.GetByIdWithDetallesAsync(request.ReservaId, ct)
            ?? throw new NotFoundException("Reserva", request.ReservaId.ToString());

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // Capturar la cantidad ANTES de quitar el servicio
            var cantidadAnterior = reserva.Servicios
                .FirstOrDefault(s => s.ServicioAdicionalId == request.ServicioAdicionalId)
                ?.Cantidad;

            reserva.QuitarServicio(request.ServicioAdicionalId);

            await _uow.SaveChangesAsync(ct);

            var evento = new AuditoriaEvento(
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
                descripcion: $"Servicio {request.ServicioAdicionalId} removido de reserva {reserva.ReservaId}.");

            // ── Detalles del cambio ───────────────────────────────────────
            evento.AgregarDetalle(
                campo: "ServicioAdicionalId",
                valorAnterior: request.ServicioAdicionalId.ToString(),
                valorNuevo: null);

            if (cantidadAnterior.HasValue)
                evento.AgregarDetalle(
                    campo: "Cantidad",
                    valorAnterior: cantidadAnterior.Value.ToString(),
                    valorNuevo: null);

            await _auditoria.RegistrarAsync(evento, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

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

        // ── 1. Leer con AsNoTracking para validaciones de dominio ─────────
        // AsNoTracking garantiza que ninguna entidad hija (DetalleReserva,
        // ReservaServicioAdicional) quede en el change tracker del DbContext.
        // Esto es crítico: si quedaran trackeadas, SaveChangesAsync generaría
        // OUTPUT clause sobre esas tablas, lo cual SQL Server rechaza por sus triggers.
        var reserva = await _reservas.GetByIdWithDetallesAsNoTrackingAsync(request.ReservaId, ct)
            ?? throw new NotFoundException("Reserva", request.ReservaId.ToString());

        var entradaAnterior = reserva.FechaEntrada;
        var salidaAnterior = reserva.FechaSalida;

        // Ejecutar todas las validaciones de dominio
        reserva.CambiarFechas(request.NuevaFechaEntrada, request.NuevaFechaSalida, _policy);

        // ── 2. Persistir con SQL directo — sin pasar por EF change tracker ─
        // EF change tracker puede tener entidades de ReservaServicioAdicional
        // cargadas por ReservaDomainPolicy (que usa GetByIdWithDetallesAsync
        // con tracking internamente). Si usamos SaveChangesAsync, EF intenta
        // hacer OUTPUT sobre esas tablas y SQL Server lo rechaza por los triggers.
        // SQL directo evita completamente el OUTPUT clause.
        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _reservas.ActualizarFechasAsync(
                request.ReservaId,
                request.NuevaFechaEntrada,
                request.NuevaFechaSalida,
                ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "UPDATE",
                modulo: "Reservas",
                entidad: "Reserva",
                entidadId: request.ReservaId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Fechas cambiadas: {entradaAnterior:d}-{salidaAnterior:d} → {request.NuevaFechaEntrada:d}-{request.NuevaFechaSalida:d}"), ct);

            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
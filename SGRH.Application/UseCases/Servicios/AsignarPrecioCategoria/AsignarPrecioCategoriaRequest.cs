using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Servicios.AsignarPrecioCategoria;

// ── Request ───────────────────────────────────────────────────────────────
public sealed record AsignarPrecioCategoriaRequest(
    int ServicioAdicionalId,
    int CategoriaHabitacionId,
    decimal Precio,
    AuditInfo AuditInfo);

// ── Response ──────────────────────────────────────────────────────────────
public sealed record AsignarPrecioCategoriaResponse(
    int ServicioAdicionalId,
    int CategoriaHabitacionId,
    decimal Precio);

// ── Validator ─────────────────────────────────────────────────────────────
public sealed class AsignarPrecioCategoriaValidator
    : IValidator<AsignarPrecioCategoriaRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        AsignarPrecioCategoriaRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.ServicioAdicionalId <= 0)
            errors.Add("El ServicioAdicionalId no es válido.");

        if (request.CategoriaHabitacionId <= 0)
            errors.Add("El CategoriaHabitacionId no es válido.");

        if (request.Precio <= 0)
            errors.Add("El precio debe ser mayor a cero.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}

// ── UseCase ───────────────────────────────────────────────────────────────
public sealed class AsignarPrecioCategoriaUseCase
{
    private readonly IServicioCategoriaPrecioRepository _precios;
    private readonly IServicioAdicionalRepository _servicios;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<AsignarPrecioCategoriaRequest> _validator;

    public AsignarPrecioCategoriaUseCase(
        IServicioCategoriaPrecioRepository precios,
        IServicioAdicionalRepository servicios,
        ICategoriaHabitacionRepository categorias,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<AsignarPrecioCategoriaRequest> validator)
    {
        _precios = precios;
        _servicios = servicios;
        _categorias = categorias;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<AsignarPrecioCategoriaResponse> ExecuteAsync(
        AsignarPrecioCategoriaRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Verificar que existen servicio y categoría ─────────────────
        var servicio = await _servicios.GetByIdAsync(request.ServicioAdicionalId, ct)
            ?? throw new NotFoundException("ServicioAdicional", request.ServicioAdicionalId.ToString());

        var categoria = await _categorias.GetByIdAsync(request.CategoriaHabitacionId, ct)
            ?? throw new NotFoundException("CategoriaHabitacion", request.CategoriaHabitacionId.ToString());

        // ── 3. Insertar o actualizar ──────────────────────────────────────
        var existente = await _precios.GetByIdAsync(
            (request.ServicioAdicionalId, request.CategoriaHabitacionId), ct);

        string accion;
        decimal? precioAnterior = null;

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            if (existente is null)
            {
                // Nuevo registro
                var nuevo = new ServicioCategoriaPrecio(
                    request.ServicioAdicionalId,
                    request.CategoriaHabitacionId,
                    request.Precio);
                await _precios.AddAsync(nuevo, ct);
                accion = "CREATE";
            }
            else
            {
                // Actualizar precio existente
                precioAnterior = existente.Precio;
                existente.ActualizarPrecio(request.Precio);
                _precios.Update(existente);
                accion = "UPDATE";
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var evento = new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: accion,
                modulo: "Servicios",
                entidad: "ServicioCategoriaPrecio",
                entidadId: $"{request.ServicioAdicionalId}-{request.CategoriaHabitacionId}",
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Precio de '{servicio.NombreServicio}' para categoría '{categoria.NombreCategoria}' {(accion == "CREATE" ? "asignado" : "actualizado")} a {request.Precio:F2}.");

            if (precioAnterior.HasValue)
                evento.AgregarDetalle("Precio",
                    precioAnterior.Value.ToString("F2"),
                    request.Precio.ToString("F2"));

            await _auditoria.RegistrarAsync(evento, ct);
            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }

        return new AsignarPrecioCategoriaResponse(
            request.ServicioAdicionalId,
            request.CategoriaHabitacionId,
            request.Precio);
    }
}
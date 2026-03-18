using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Tarifas.CrearTarifa;

public sealed class CrearTarifaUseCase
{
    private readonly ITarifaTemporadaRepository _tarifas;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly ITemporadaRepository _temporadas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<CrearTarifaRequest> _validator;

    public CrearTarifaUseCase(
        ITarifaTemporadaRepository tarifas,
        ICategoriaHabitacionRepository categorias,
        ITemporadaRepository temporadas,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria,
        IValidator<CrearTarifaRequest> validator)
    {
        _tarifas = tarifas;
        _categorias = categorias;
        _temporadas = temporadas;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<CrearTarifaResponse> ExecuteAsync(
        CrearTarifaRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar — fuera de transacción ─────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Lecturas de verificación — fuera de transacción ────────────
        var categoria = await _categorias.GetByIdAsync(request.CategoriaHabitacionId, ct)
            ?? throw new NotFoundException(
                "CategoriaHabitacion", request.CategoriaHabitacionId.ToString());

        var temporada = await _temporadas.GetByIdAsync(request.TemporadaId, ct)
            ?? throw new NotFoundException("Temporada", request.TemporadaId.ToString());

        if (await _tarifas.ExisteParaCategoriaYTemporadaAsync(
                request.CategoriaHabitacionId, request.TemporadaId, ct))
            throw new ConflictException(
                $"Ya existe una tarifa para la categoría '{categoria.NombreCategoria}' en la temporada '{temporada.NombreTemporada}'.");

        // ── 3. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var tarifa = new TarifaTemporada(
                request.CategoriaHabitacionId,
                request.TemporadaId,
                request.PrecioNoche);

            await _tarifas.AddAsync(tarifa, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "CREATE",
                modulo: "Tarifas",
                entidad: "TarifaTemporada",
                entidadId: tarifa.TarifaTemporadaId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Tarifa creada: '{categoria.NombreCategoria}' en '{temporada.NombreTemporada}' = {request.PrecioNoche:C}/noche."), ct);

            await _unitOfWork.CommitAsync(ct);
            return new CrearTarifaResponse(
                TarifaMapper.ToDto(tarifa, categoria.NombreCategoria, temporada.NombreTemporada));
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
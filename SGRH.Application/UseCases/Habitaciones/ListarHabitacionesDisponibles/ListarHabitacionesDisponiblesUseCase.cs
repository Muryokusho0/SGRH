using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.ListarHabitacionesDisponibles;

public sealed class ListarHabitacionesDisponiblesUseCase
{
    private readonly IHabitacionRepository _habitaciones;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly IValidator<ListarHabitacionesDisponiblesRequest> _validator;

    public ListarHabitacionesDisponiblesUseCase(
        IHabitacionRepository habitaciones,
        ICategoriaHabitacionRepository categorias,
        IValidator<ListarHabitacionesDisponiblesRequest> validator)
    {
        _habitaciones = habitaciones;
        _categorias = categorias;
        _validator = validator;
    }

    public async Task<ListarHabitacionesDisponiblesResponse> ExecuteAsync(
        ListarHabitacionesDisponiblesRequest request,
        CancellationToken ct = default)
    {
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Consultar habitaciones disponibles en el rango ─────────────
        // El repositorio filtra: estado Disponible + sin reserva activa en el rango
        var habitaciones = await _habitaciones.GetDisponiblesAsync(
            request.FechaEntrada,
            request.FechaSalida,
            request.CategoriaHabitacionId,
            ct);

        // ── 3. Cargar nombres de categorías para el mapper ─────────────────
        var todasCategorias = await _categorias.GetAllAsync(ct);
        var dicCategorias = todasCategorias
            .ToDictionary(c => c.CategoriaHabitacionId, c => c.NombreCategoria);

        return new ListarHabitacionesDisponiblesResponse(
            HabitacionMapper.ToDtoList(habitaciones, dicCategorias));
    }
}

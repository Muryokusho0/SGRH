using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Servicios.ListarServiciosPorCategoria;

// Devuelve los servicios disponibles para una categoría con su precio aplicable.
// Diferente a ListarServicios — aquí el precio varía por categoría (regla del SRS 5.4).
// Se usa en el flujo de reservas para mostrar al cliente los servicios con precio correcto.
public sealed class ListarServiciosPorCategoriaUseCase
{
    private readonly IServicioAdicionalRepository _servicios;
    private readonly IServicioCategoriaPrecioRepository _precios;
    private readonly ICategoriaHabitacionRepository _categorias;

    public ListarServiciosPorCategoriaUseCase(
        IServicioAdicionalRepository servicios,
        IServicioCategoriaPrecioRepository precios,
        ICategoriaHabitacionRepository categorias)
    {
        _servicios = servicios;
        _precios = precios;
        _categorias = categorias;
    }

    public async Task<ListarServiciosPorCategoriaResponse> ExecuteAsync(
        int categoriaHabitacionId, CancellationToken ct = default)
    {
        // ── 1. Verificar que la categoría existe ──────────────────────────
        var categoria = await _categorias.GetByIdAsync(categoriaHabitacionId, ct)
            ?? throw new NotFoundException(
                "CategoriaHabitacion", categoriaHabitacionId.ToString());

        // ── 2. Obtener todos los servicios ────────────────────────────────
        var todosServicios = await _servicios.GetAllAsync(ct);

        // ── 3. Para cada servicio, obtener su precio para esta categoría ──
        // Solo se incluyen servicios que tienen precio definido para la categoría.
        var result = new List<ServicioPorCategoriaDto>();

        foreach (var servicio in todosServicios)
        {
            var precio = await _precios.GetPrecioAsync(
                servicio.ServicioAdicionalId, categoriaHabitacionId, ct);

            // Si no hay precio definido para esta categoría, el servicio no aplica
            if (precio is null) continue;

            result.Add(new ServicioPorCategoriaDto(
                ServicioAdicionalId: servicio.ServicioAdicionalId,
                NombreServicio: servicio.NombreServicio,
                Descripcion: servicio.TipoServicio,
                PrecioPorCategoria: precio.Value));
        }

        return new ListarServiciosPorCategoriaResponse(
            CategoriaHabitacionId: categoriaHabitacionId,
            Servicios: result);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para categorías de habitación.
/// Proporciona búsqueda por nombre y capacidad además de las operaciones CRUD básicas.
/// </summary>
public interface ICategoriaHabitacionRepository : IRepository<CategoriaHabitacion, int>
{
    /// <summary>
    /// Verifica si ya existe una categoría de habitación con el nombre indicado.
    /// </summary>
    /// <param name="nombre">Nombre a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si ya existe una categoría con ese nombre; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExistsByNombreAsync(string nombre, CancellationToken ct = default);

    /// <summary>
    /// Busca categorías de habitación con filtros opcionales de nombre y capacidad.
    /// </summary>
    /// <param name="nombre">Filtrar por nombre (búsqueda parcial, opcional).</param>
    /// <param name="capacidadMinima">Capacidad mínima requerida (opcional).</param>
    /// <param name="capacidadMaxima">Capacidad máxima permitida (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de categorías que cumplen los filtros indicados.</returns>
    Task<List<CategoriaHabitacion>> BuscarAsync(
        string? nombre,
        int? capacidadMinima,
        int? capacidadMaxima,
        CancellationToken ct = default);
}

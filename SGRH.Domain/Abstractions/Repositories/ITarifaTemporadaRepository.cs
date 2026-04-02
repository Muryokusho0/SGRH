using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para tarifas de temporada por categoría de habitación.
/// Permite verificar existencia, obtener la tarifa activa y buscar tarifas con filtros.
/// </summary>
public interface ITarifaTemporadaRepository : IRepository<TarifaTemporada, int>
{
    /// <summary>
    /// Verifica si ya existe una tarifa configurada para una categoría de habitación y temporada específicas.
    /// </summary>
    /// <param name="categoriaHabitacionId">Id de la categoría de habitación.</param>
    /// <param name="temporadaId">Id de la temporada.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si ya existe la tarifa para esa combinación; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExisteParaCategoriaYTemporadaAsync(
        int categoriaHabitacionId, int temporadaId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene la tarifa de temporada para una categoría de habitación y temporada específicas.
    /// </summary>
    /// <param name="categoriaId">Id de la categoría de habitación.</param>
    /// <param name="temporadaId">Id de la temporada.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La tarifa encontrada, o <c>null</c> si no existe configuración para esa combinación.</returns>
    Task<TarifaTemporada?> GetTarifaAsync(
        int categoriaId, int temporadaId, CancellationToken ct = default);

    /// <summary>
    /// Busca tarifas de temporada con filtros opcionales de categoría y temporada.
    /// </summary>
    /// <param name="categoriaId">Filtrar por categoría de habitación (opcional).</param>
    /// <param name="temporadaId">Filtrar por temporada (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de tarifas que cumplen los filtros indicados.</returns>
    Task<List<TarifaTemporada>> BuscarAsync(
        int? categoriaId, int? temporadaId, CancellationToken ct = default);
}
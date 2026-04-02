using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Temporadas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para temporadas del hotel.
/// Permite obtener la temporada activa para una fecha, verificar solapamientos y buscar con filtros.
/// </summary>
public interface ITemporadaRepository : IRepository<Temporada, int>
{
    /// <summary>
    /// Obtiene la temporada cuyo rango contiene la fecha especificada, si existe.
    /// Considera tanto temporadas específicas como recurrentes.
    /// </summary>
    /// <param name="fecha">Fecha para la que se busca la temporada activa.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La temporada que contiene la fecha, o <c>null</c> si ninguna aplica.</returns>
    Task<Temporada?> GetByFechaAsync(DateTime fecha, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe alguna temporada específica (no recurrente) cuyo rango se solape
    /// con el indicado, excluyendo opcionalmente la temporada con el id dado.
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio del rango a verificar.</param>
    /// <param name="fechaFin">Fecha de fin del rango a verificar.</param>
    /// <param name="excludeId">Id de temporada a excluir de la verificación (para ediciones).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si existe solapamiento; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExisteSolapamientoAsync(
        DateTime fechaInicio, DateTime fechaFin, int? excludeId,
        CancellationToken ct = default);

    /// <summary>
    /// Busca temporadas con filtros opcionales de nombre y rango de fechas.
    /// </summary>
    /// <param name="nombre">Filtrar por nombre (búsqueda parcial, opcional).</param>
    /// <param name="fechaDesde">Filtrar temporadas que inicien desde esta fecha (opcional).</param>
    /// <param name="fechaHasta">Filtrar temporadas que finalicen hasta esta fecha (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de temporadas que cumplen los filtros indicados.</returns>
    Task<List<Temporada>> BuscarAsync(
        string? nombre, DateTime? fechaDesde, DateTime? fechaHasta,
        CancellationToken ct = default);
}

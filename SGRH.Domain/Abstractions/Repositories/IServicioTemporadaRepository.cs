using SGRH.Domain.Entities.Servicios;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio para la tabla de unión entre servicios adicionales y temporadas.
/// Permite verificar y registrar la disponibilidad de un servicio en una temporada específica.
/// </summary>
public interface IServicioTemporadaRepository
{
    /// <summary>
    /// Verifica si ya existe una asignación del servicio a la temporada indicada.
    /// </summary>
    /// <param name="servicioId">Id del servicio adicional.</param>
    /// <param name="temporadaId">Id de la temporada.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si ya existe la asignación; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExisteAsync(int servicioId, int temporadaId, CancellationToken ct = default);

    /// <summary>
    /// Agrega una nueva asignación entre un servicio adicional y una temporada.
    /// </summary>
    /// <param name="entity">Entidad de unión a registrar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(ServicioTemporada entity, CancellationToken ct = default);
}
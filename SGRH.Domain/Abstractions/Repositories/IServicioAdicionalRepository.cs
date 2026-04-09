using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Servicios;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para servicios adicionales del hotel.
/// </summary>
public interface IServicioAdicionalRepository : IRepository<ServicioAdicional, int>
{
    /// <summary>
    /// Verifica si ya existe un servicio adicional con el nombre indicado.
    /// </summary>
    Task<bool> ExistsByNombreAsync(string nombreServicio, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un servicio adicional por su id, incluyendo las temporadas asociadas.
    /// </summary>
    Task<ServicioAdicional?> GetByIdWithTemporadasAsync(
        int id, CancellationToken ct = default);

    /// <summary>
    /// Busca servicios filtrando opcionalmente por nombre (sin filtro de temporada).
    /// </summary>
    Task<List<ServicioAdicional>> BuscarAsync(
        string? nombre, CancellationToken ct = default);

    /// <summary>
    /// Busca servicios disponibles según la temporada activa.
    ///
    /// Devuelve servicios que cumplen al menos una de estas condiciones:
    ///   a) AplicaTodasTemporadas = true (disponibles siempre).
    ///   b) Están asignados a la temporada indicada en ServicioTemporada.
    ///
    /// Si temporadaId es null (sin temporada activa), devuelve todos los
    /// servicios sin restricción estacional (misma lógica que el dominio).
    /// </summary>
    /// <param name="nombre">Filtro opcional por nombre (búsqueda parcial).</param>
    /// <param name="temporadaId">
    ///     Id de la temporada activa. Si es null, no se aplica filtro por temporada.
    /// </param>
    Task<List<ServicioAdicional>> BuscarDisponiblesAsync(
        string? nombre, int? temporadaId, CancellationToken ct = default);
}
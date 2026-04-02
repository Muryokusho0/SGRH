using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para servicios adicionales del hotel.
/// Permite verificar duplicados por nombre, cargar temporadas asociadas y búsqueda filtrada.
/// </summary>
public interface IServicioAdicionalRepository : IRepository<ServicioAdicional, int>
{
    /// <summary>
    /// Verifica si ya existe un servicio adicional con el nombre indicado.
    /// </summary>
    /// <param name="nombreServicio">Nombre del servicio a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si ya existe un servicio con ese nombre; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExistsByNombreAsync(string nombreServicio, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un servicio adicional por su id, incluyendo las temporadas asociadas cargadas.
    /// </summary>
    /// <param name="id">Id del servicio adicional.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El servicio con sus temporadas cargadas, o <c>null</c> si no existe.</returns>
    Task<ServicioAdicional?> GetByIdWithTemporadasAsync(
        int id, CancellationToken ct = default);

    /// <summary>
    /// Busca servicios adicionales filtrando opcionalmente por nombre.
    /// </summary>
    /// <param name="nombre">Filtrar por nombre (búsqueda parcial, opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de servicios que cumplen el filtro indicado.</returns>
    Task<List<ServicioAdicional>> BuscarAsync(
        string? nombre, CancellationToken ct = default);
}

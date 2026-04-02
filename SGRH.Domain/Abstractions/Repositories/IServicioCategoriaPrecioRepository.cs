using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para los precios de servicios adicionales por categoría de habitación.
/// Permite consultar el precio unitario de un servicio para una categoría específica.
/// </summary>
public interface IServicioCategoriaPrecioRepository
    : IRepository<ServicioCategoriaPrecio, (int ServicioId, int CategoriaId)>
{
    /// <summary>
    /// Obtiene el precio unitario de un servicio adicional para una categoría de habitación específica.
    /// </summary>
    /// <param name="servicioId">Id del servicio adicional.</param>
    /// <param name="categoriaId">Id de la categoría de habitación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El precio unitario configurado, o <c>null</c> si no existe precio para esa combinación.
    /// </returns>
    Task<decimal?> GetPrecioAsync(
        int servicioId, int categoriaId, CancellationToken ct = default);
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Clientes;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para clientes del hotel.
/// Permite búsqueda por documento de identidad, correo y nombre.
/// </summary>
public interface IClienteRepository : IRepository<Cliente, int>
{
    /// <summary>
    /// Busca un cliente por su número de documento de identidad nacional.
    /// </summary>
    /// <param name="nationalId">Número de documento de identidad (cédula, pasaporte, etc.).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El cliente encontrado, o <c>null</c> si no existe.</returns>
    Task<Cliente?> GetByNationalIdAsync(string nationalId, CancellationToken ct = default);

    /// <summary>
    /// Busca un cliente por su dirección de correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico del cliente.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El cliente encontrado, o <c>null</c> si no existe.</returns>
    Task<Cliente?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Busca clientes con filtros opcionales de nombre, correo y documento de identidad.
    /// </summary>
    /// <param name="nombre">Filtrar por nombre o apellido (búsqueda parcial, opcional).</param>
    /// <param name="email">Filtrar por correo electrónico (búsqueda parcial, opcional).</param>
    /// <param name="nationalId">Filtrar por documento de identidad (búsqueda parcial, opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de clientes que cumplen los filtros indicados.</returns>
    Task<List<Cliente>> BuscarAsync(
        string? nombre,
        string? email,
        string? nationalId,
        CancellationToken ct = default);
}

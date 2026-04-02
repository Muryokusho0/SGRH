using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Define las operaciones CRUD básicas del repositorio genérico del dominio.
/// Todos los repositorios específicos extienden esta interfaz.
/// </summary>
/// <typeparam name="T">Tipo de la entidad que maneja el repositorio.</typeparam>
/// <typeparam name="TKey">Tipo de la clave primaria de la entidad.</typeparam>
public interface IRepository<T, TKey> where T : class
{
    /// <summary>
    /// Obtiene una entidad por su clave primaria.
    /// </summary>
    /// <param name="id">Clave primaria de la entidad a buscar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La entidad encontrada, o <c>null</c> si no existe.</returns>
    Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las entidades del repositorio.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista con todas las entidades almacenadas.</returns>
    Task<List<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Agrega una nueva entidad al repositorio (persiste en el siguiente <c>SaveChanges</c>).
    /// </summary>
    /// <param name="entity">Entidad a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Marca una entidad existente como modificada (persiste en el siguiente <c>SaveChanges</c>).
    /// </summary>
    /// <param name="entity">Entidad con los datos actualizados.</param>
    void Update(T entity);

    /// <summary>
    /// Marca una entidad para eliminación (persiste en el siguiente <c>SaveChanges</c>).
    /// </summary>
    /// <param name="entity">Entidad a eliminar.</param>
    void Delete(T entity);
}

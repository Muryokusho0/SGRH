using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Define el contrato del patrón Unit of Work para coordinar la persistencia
/// de múltiples repositorios en una única transacción de base de datos.
/// La implementación reside en la capa de persistencia (EF Core).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Número de registros afectados en la base de datos.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Inicia una transacción explícita en la base de datos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Confirma todos los cambios de la transacción actual y la cierra.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Revierte todos los cambios de la transacción actual y la cierra.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    Task RollbackAsync(CancellationToken ct = default);
}

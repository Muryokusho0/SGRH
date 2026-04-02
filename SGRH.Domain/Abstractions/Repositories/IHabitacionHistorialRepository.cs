using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para el historial de estados de habitaciones.
/// Permite consultar el registro de historial vigente (estado actual) de una habitación.
/// </summary>
public interface IHabitacionHistorialRepository : IRepository<HabitacionHistorial, int>
{
    /// <summary>
    /// Obtiene el registro de historial vigente de una habitación (el que tiene <c>FechaFin == null</c>).
    /// </summary>
    /// <param name="habitacionId">Id de la habitación cuyo estado actual se desea consultar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El registro de historial vigente, o <c>null</c> si no existe ningún registro abierto
    /// (lo cual indicaría un problema de integridad de datos).
    /// </returns>
    Task<HabitacionHistorial?> GetVigenteAsync(
        int habitacionId, CancellationToken ct = default);
}
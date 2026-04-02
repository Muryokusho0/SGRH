using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Auditoria;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para eventos de auditoría.
/// Extiende el repositorio genérico con consultas filtradas para análisis de trazabilidad.
/// </summary>
public interface IAuditoriaRepository : IRepository<AuditoriaEvento, long>
{
    /// <summary>
    /// Obtiene todos los eventos de auditoría generados por un usuario específico.
    /// </summary>
    /// <param name="usuarioId">Id del usuario cuyos eventos se desean consultar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de eventos de auditoría del usuario.</returns>
    Task<List<AuditoriaEvento>> GetByUsuarioAsync(
        int usuarioId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los eventos de auditoría de un módulo del sistema.
    /// </summary>
    /// <param name="modulo">Nombre del módulo (por ejemplo: "Reservas", "Habitaciones").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de eventos del módulo indicado.</returns>
    Task<List<AuditoriaEvento>> GetByModuloAsync(
        string modulo, CancellationToken ct = default);

    /// <summary>
    /// Obtiene los eventos de auditoría dentro de un rango de fechas.
    /// </summary>
    /// <param name="desde">Fecha de inicio del rango (inclusiva).</param>
    /// <param name="hasta">Fecha de fin del rango (inclusiva).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de eventos en el rango de fechas indicado.</returns>
    Task<List<AuditoriaEvento>> GetByRangoFechaAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default);

    /// <summary>
    /// Busca eventos de auditoría con múltiples filtros opcionales combinados.
    /// </summary>
    /// <param name="modulo">Filtrar por módulo (opcional).</param>
    /// <param name="accion">Filtrar por acción ejecutada (opcional).</param>
    /// <param name="entidad">Filtrar por tipo de entidad (opcional).</param>
    /// <param name="usuarioId">Filtrar por usuario (opcional).</param>
    /// <param name="fechaDesde">Fecha de inicio del rango (opcional).</param>
    /// <param name="fechaHasta">Fecha de fin del rango (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de eventos que cumplen todos los filtros aplicados.</returns>
    Task<List<AuditoriaEvento>> BuscarAsync(
        string? modulo,
        string? accion,
        string? entidad,
        int? usuarioId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken ct = default);
}
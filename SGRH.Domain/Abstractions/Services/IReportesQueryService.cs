using SGRH.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services;

/// <summary>
/// Resultado de la consulta de ocupación activa de habitaciones.
/// Representa una habitación ocupada en una reserva activa.
/// </summary>
/// <param name="ReservaId">Identificador de la reserva.</param>
/// <param name="HabitacionId">Identificador de la habitación ocupada.</param>
/// <param name="HabitacionCodigo">Código o número visible de la habitación.</param>
/// <param name="CategoriaHabitacionId">Identificador de la categoría de la habitación.</param>
/// <param name="CategoriaNombre">Nombre de la categoría de la habitación.</param>
/// <param name="FechaEntrada">Fecha de check-in de la reserva.</param>
/// <param name="FechaSalida">Fecha de check-out de la reserva.</param>
/// <param name="EstadoReserva">Estado actual de la reserva (por ejemplo: "Confirmada").</param>
/// <param name="ClienteNombre">Nombre completo del cliente que realizó la reserva.</param>
public sealed record OcupacionActivaResult(
    int ReservaId,
    int HabitacionId,
    string HabitacionCodigo,
    int CategoriaHabitacionId,
    string CategoriaNombre,
    DateTime FechaEntrada,
    DateTime FechaSalida,
    string EstadoReserva,
    string ClienteNombre);

/// <summary>
/// Resultado de la consulta de costos totales de una reserva,
/// desglosado entre habitaciones y servicios adicionales.
/// </summary>
/// <param name="ReservaId">Identificador de la reserva.</param>
/// <param name="ClienteId">Identificador del cliente.</param>
/// <param name="ClienteNombre">Nombre completo del cliente.</param>
/// <param name="FechaEntrada">Fecha de check-in.</param>
/// <param name="FechaSalida">Fecha de check-out.</param>
/// <param name="TotalHabitaciones">Suma de las tarifas de todas las habitaciones en la reserva.</param>
/// <param name="TotalServicios">Suma de los subtotales de todos los servicios adicionales.</param>
public sealed record ReservaCostoTotalResult(
    int ReservaId,
    int ClienteId,
    string ClienteNombre,
    DateTime FechaEntrada,
    DateTime FechaSalida,
    decimal TotalHabitaciones,
    decimal TotalServicios);

/// <summary>
/// Resultado de la consulta de uso de servicios adicionales,
/// mostrando la demanda e ingresos generados por cada servicio.
/// </summary>
/// <param name="ServicioAdicionalId">Identificador del servicio adicional.</param>
/// <param name="ServicioNombre">Nombre del servicio adicional.</param>
/// <param name="CantidadSolicitudes">Número de veces que el servicio fue contratado.</param>
/// <param name="IngresoTotal">Ingreso total generado por el servicio en el período consultado.</param>
public sealed record UsoServiciosResult(
    int ServicioAdicionalId,
    string ServicioNombre,
    int CantidadSolicitudes,
    decimal IngresoTotal);

/// <summary>
/// Parámetros de filtro utilizados en las consultas del servicio de reportes.
/// Todos los filtros son opcionales y se combinan para acotar los resultados.
/// </summary>
/// <param name="Desde">Fecha de inicio del rango de consulta (opcional).</param>
/// <param name="Hasta">Fecha de fin del rango de consulta (opcional).</param>
/// <param name="CategoriaHabitacionId">Filtrar por categoría de habitación (opcional).</param>
/// <param name="HabitacionId">Filtrar por habitación específica (opcional).</param>
/// <param name="ServicioAdicionalId">Filtrar por servicio adicional (opcional).</param>
/// <param name="ClienteId">Filtrar por cliente (opcional).</param>
/// <param name="Top">Limitar el número de resultados retornados (opcional).</param>
public sealed record ReportesQueryFiltros(
    DateTime? Desde = null,
    DateTime? Hasta = null,
    int? CategoriaHabitacionId = null,
    int? HabitacionId = null,
    int? ServicioAdicionalId = null,
    int? ClienteId = null,
    int? Top = null);

/// <summary>
/// Define el contrato del servicio de consultas para generación de reportes operativos.
/// La implementación reside en la capa de persistencia (<c>ReportesQueryService</c>),
/// desacoplando la capa de aplicación de la implementación concreta de las consultas.
/// </summary>
public interface IReportesQueryService
{
    /// <summary>
    /// Obtiene el reporte de ocupación activa de habitaciones, con filtros opcionales.
    /// </summary>
    /// <param name="filtros">Filtros de búsqueda (rango de fechas, categoría, habitación, etc.).</param>
    /// <param name="estado">Filtrar por estado de reserva (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de registros de ocupación que cumplen los filtros.</returns>
    Task<List<OcupacionActivaResult>> GetOcupacionActivaAsync(
        ReportesQueryFiltros filtros,
        EstadoReserva? estado = null,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene el reporte de costos totales de reservas, desglosado por habitaciones y servicios.
    /// </summary>
    /// <param name="filtros">Filtros de búsqueda (rango de fechas, cliente, etc.).</param>
    /// <param name="estado">Filtrar por estado de reserva (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de registros de costos de reservas que cumplen los filtros.</returns>
    Task<List<ReservaCostoTotalResult>> GetReservaCostoTotalAsync(
        ReportesQueryFiltros filtros,
        EstadoReserva? estado = null,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene el reporte de uso y demanda de servicios adicionales.
    /// </summary>
    /// <param name="filtros">Filtros de búsqueda (rango de fechas, servicio, etc.).</param>
    /// <param name="estado">Filtrar por estado de reserva (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de registros de uso de servicios que cumplen los filtros.</returns>
    Task<List<UsoServiciosResult>> GetUsoServiciosAsync(
        ReportesQueryFiltros filtros,
        EstadoReserva? estado = null,
        CancellationToken ct = default);
}

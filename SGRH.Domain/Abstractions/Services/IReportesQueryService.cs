using SGRH.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services;

// ── Tipos de resultado propios de Application ─────────────────────────────────
// Desacoplados de SGRH.Persistence.Queries.Models para respetar la arquitectura.
// ReportesQueryService en Persistence los implementa y los mapea a estos tipos.

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

public sealed record ReservaCostoTotalResult(
    int ReservaId,
    int ClienteId,
    string ClienteNombre,
    DateTime FechaEntrada,
    DateTime FechaSalida,
    decimal TotalHabitaciones,
    decimal TotalServicios);

public sealed record UsoServiciosResult(
    int ServicioAdicionalId,
    string ServicioNombre,
    int CantidadSolicitudes,
    decimal IngresoTotal);

public sealed record ReportesQueryFiltros(
    DateTime? Desde = null,
    DateTime? Hasta = null,
    int? CategoriaHabitacionId = null,
    int? HabitacionId = null,
    int? ServicioAdicionalId = null,
    int? ClienteId = null,
    int? Top = null);

// ── Contrato ──────────────────────────────────────────────────────────────────
// Implementado en SGRH.Persistence por ReportesQueryService.
// Application solo conoce esta interfaz, nunca la implementación.
public interface IReportesQueryService
{
    Task<List<OcupacionActivaResult>> GetOcupacionActivaAsync(
        ReportesQueryFiltros filtros,
        EstadoReserva? estado = null,
        CancellationToken ct = default);

    Task<List<ReservaCostoTotalResult>> GetReservaCostoTotalAsync(
        ReportesQueryFiltros filtros,
        EstadoReserva? estado = null,
        CancellationToken ct = default);

    Task<List<UsoServiciosResult>> GetUsoServiciosAsync(
        ReportesQueryFiltros filtros,
        EstadoReserva? estado = null,
        CancellationToken ct = default);
}

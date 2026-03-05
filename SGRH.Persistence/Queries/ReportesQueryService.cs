using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Enums;
using SGRH.Persistence.Context;
using SGRH.Persistence.Queries.Models;

namespace SGRH.Persistence.Queries;

public sealed class ReportesQueryService
{
    private readonly SGRHDbContext _db;

    public ReportesQueryService(SGRHDbContext db) => _db = db;

    /// <summary>
    /// Reporte: Ocupación (RF-16).
    /// Devuelve filas de ocupación en un rango de fechas, con filtros opcionales.
    /// </summary>
    public async Task<List<OcupacionActivaRow>> GetOcupacionActivaAsync(
        ReportesConfiguration cfg,
        EstadoReserva? estado = null,
        CancellationToken ct = default)
    {
        var desde = cfg.Desde ?? DateTime.Today;
        var hasta = cfg.Hasta ?? DateTime.Today;

        // Reservas solapadas en el rango
        var reservas = _db.Reservas.AsNoTracking()
            .Where(r => r.FechaEntrada <= hasta && r.FechaSalida >= desde);

        if (estado.HasValue)
            reservas = reservas.Where(r => r.EstadoReserva == estado.Value);

        var query =
            from r in reservas
            join dr in _db.DetallesReserva.AsNoTracking()
                on r.ReservaId equals dr.ReservaId
            join h in _db.Habitaciones.AsNoTracking()
                on dr.HabitacionId equals h.HabitacionId
            join c in _db.CategoriasHabitacion.AsNoTracking()
                on h.CategoriaHabitacionId equals c.CategoriaHabitacionId
            select new OcupacionActivaRow
            {
                ReservaId = r.ReservaId,
                HabitacionId = h.HabitacionId,
                CategoriaHabitacionId = c.CategoriaHabitacionId,
                FechaEntrada = r.FechaEntrada,
                FechaSalida = r.FechaSalida,
            };

        if (cfg.CategoriaHabitacionId.HasValue)
            query = query.Where(x => x.CategoriaHabitacionId == cfg.CategoriaHabitacionId.Value);

        if (cfg.HabitacionId.HasValue)
            query = query.Where(x => x.HabitacionId == cfg.HabitacionId.Value);

        return await query
            .OrderBy(x => x.FechaEntrada)
            .ThenBy(x => x.HabitacionId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Reporte: Ingresos/Costo total por reserva (RF-16), cumpliendo RF-10 y RF-12.
    /// </summary>
    public async Task<List<ReservaCostoTotalRow>> GetReservaCostoTotalAsync(
        ReportesConfiguration cfg,
        EstadoReserva? estado = null,
        CancellationToken ct = default)
    {
        var desde = cfg.Desde ?? DateTime.MinValue;
        var hasta = cfg.Hasta ?? DateTime.MaxValue;

        var reservas = _db.Reservas.AsNoTracking()
            .Where(r => r.FechaReserva >= desde && r.FechaReserva <= hasta);

        if (cfg.ClienteId.HasValue)
            reservas = reservas.Where(r => r.ClienteId == cfg.ClienteId.Value);

        if (estado.HasValue)
            reservas = reservas.Where(r => r.EstadoReserva == estado.Value);

        var reservaIds = await reservas.Select(r => r.ReservaId).ToListAsync(ct);

        // Habitaciones (tarifa snapshot)
        var totHab = await _db.DetallesReserva.AsNoTracking()
            .Where(dr => reservaIds.Contains(dr.ReservaId))
            .GroupBy(dr => dr.ReservaId)
            .Select(g => new { ReservaId = g.Key, Total = g.Sum(x => x.TarifaAplicada) })
            .ToListAsync(ct);

        // Servicios (precio snapshot)
        var totSrv = await _db.ReservaServiciosAdicionales.AsNoTracking()
            .Where(rs => reservaIds.Contains(rs.ReservaId))
            .GroupBy(rs => rs.ReservaId)
            .Select(g => new { ReservaId = g.Key, Total = g.Sum(x => x.Cantidad * x.PrecioUnitarioAplicado) })
            .ToListAsync(ct);

        var mapHab = totHab.ToDictionary(x => x.ReservaId, x => x.Total);
        var mapSrv = totSrv.ToDictionary(x => x.ReservaId, x => x.Total);

        return [.. reservaIds.Select(id => new ReservaCostoTotalRow
        {
            ReservaId = id,
            TotalHabitaciones = mapHab.TryGetValue(id, out var th) ? th : 0m,
            TotalServicios = mapSrv.TryGetValue(id, out var ts) ? ts : 0m
        })];
    }

    /// <summary>
    /// Reporte: Uso de servicios (RF-16). Ranking por ingresos.
    /// </summary>
    public async Task<List<UsoServiciosRow>> GetUsoServiciosAsync(
        ReportesConfiguration cfg,
        EstadoReserva? estado = null,
        CancellationToken ct = default)
    {
        var desde = cfg.Desde ?? DateTime.MinValue;
        var hasta = cfg.Hasta ?? DateTime.MaxValue;

        var reservas = _db.Reservas.AsNoTracking()
            .Where(r => r.FechaReserva >= desde && r.FechaReserva <= hasta);

        if (estado.HasValue)
            reservas = reservas.Where(r => r.EstadoReserva == estado.Value);

        var query =
            from rs in _db.ReservaServiciosAdicionales.AsNoTracking()
            join r in reservas on rs.ReservaId equals r.ReservaId
            join s in _db.ServiciosAdicionales.AsNoTracking()
                on rs.ServicioAdicionalId equals s.ServicioAdicionalId
            select new { rs, s };

        if (cfg.ServicioAdicionalId.HasValue)
            query = query.Where(x => x.s.ServicioAdicionalId == cfg.ServicioAdicionalId.Value);

        var rows = await query
            .GroupBy(x => x.s.ServicioAdicionalId)
            .Select(g => new UsoServiciosRow
            {
                ServicioAdicionalId = g.Key,
                CantidadSolicitudes = g.Sum(x => x.rs.Cantidad),
                IngresoTotal = g.Sum(x => x.rs.Cantidad * x.rs.PrecioUnitarioAplicado),
            })
            .OrderByDescending(x => x.IngresoTotal)
            .ToListAsync(ct);

        if (cfg.Top.HasValue && cfg.Top.Value > 0)
            rows = [.. rows.Take(cfg.Top.Value)];

        return rows;
    }
}
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

    // ─────────────────────────────────────────────────────────────────────────
    // RF-16: Ocupación
    // Habitaciones con reservas solapadas en el rango, con datos de cliente.
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<OcupacionActivaRow>> GetOcupacionActivaAsync(
        ReportesConfiguration cfg,
        EstadoReserva? estado = null,
        CancellationToken ct = default)
    {
        var desde = cfg.Desde ?? DateTime.Today;
        var hasta = cfg.Hasta ?? DateTime.Today;

        var reservas = _db.Reservas.AsNoTracking()
            .Where(r => r.FechaEntrada <= hasta && r.FechaSalida >= desde);

        if (estado.HasValue)
            reservas = reservas.Where(r => r.EstadoReserva == estado.Value);

        // Query raw — evita problemas de traducción SQL con ToString() y enum
        var rawQuery =
            from r in reservas
            join dr in _db.DetallesReserva.AsNoTracking() on r.ReservaId equals dr.ReservaId
            join h in _db.Habitaciones.AsNoTracking() on dr.HabitacionId equals h.HabitacionId
            join c in _db.CategoriasHabitacion.AsNoTracking() on h.CategoriaHabitacionId equals c.CategoriaHabitacionId
            join cli in _db.Clientes.AsNoTracking() on r.ClienteId equals cli.ClienteId
            select new
            {
                r.ReservaId,
                h.HabitacionId,
                h.NumeroHabitacion,
                c.CategoriaHabitacionId,
                c.NombreCategoria,
                r.FechaEntrada,
                r.FechaSalida,
                r.EstadoReserva,
                cli.NombreCliente,
                cli.ApellidoCliente,
            };

        if (cfg.CategoriaHabitacionId.HasValue)
            rawQuery = rawQuery.Where(x => x.CategoriaHabitacionId == cfg.CategoriaHabitacionId.Value);

        if (cfg.HabitacionId.HasValue)
            rawQuery = rawQuery.Where(x => x.HabitacionId == cfg.HabitacionId.Value);

        var raw = await rawQuery
            .OrderBy(x => x.FechaEntrada)
            .ThenBy(x => x.HabitacionId)
            .ToListAsync(ct);

        // Proyección en memoria — ToString() y concatenación sin riesgo de traducción
        return raw.Select(x => new OcupacionActivaRow
        {
            ReservaId = x.ReservaId,
            HabitacionId = x.HabitacionId,
            HabitacionCodigo = x.NumeroHabitacion.ToString(),
            CategoriaHabitacionId = x.CategoriaHabitacionId,
            CategoriaNombre = x.NombreCategoria,
            FechaEntrada = x.FechaEntrada,
            FechaSalida = x.FechaSalida,
            EstadoReserva = x.EstadoReserva.ToString(),
            ClienteNombre = $"{x.NombreCliente} {x.ApellidoCliente}",
        }).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RF-16: Costo total por reserva (snapshot de precios — RF-10, RF-12)
    // ─────────────────────────────────────────────────────────────────────────
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

        // Info de reserva + cliente en un solo round-trip
        var reservaInfo = await (
            from r in reservas
            join cli in _db.Clientes.AsNoTracking() on r.ClienteId equals cli.ClienteId
            select new
            {
                r.ReservaId,
                r.ClienteId,
                r.FechaEntrada,
                r.FechaSalida,
                cli.NombreCliente,
                cli.ApellidoCliente,
            }
        ).ToListAsync(ct);

        var reservaIds = reservaInfo.Select(x => x.ReservaId).ToList();

        // Subtotales habitaciones (TarifaAplicada = snapshot)
        var totHab = await _db.DetallesReserva.AsNoTracking()
            .Where(dr => reservaIds.Contains(dr.ReservaId))
            .GroupBy(dr => dr.ReservaId)
            .Select(g => new { ReservaId = g.Key, Total = g.Sum(x => x.TarifaAplicada) })
            .ToListAsync(ct);

        // Subtotales servicios (PrecioUnitarioAplicado = snapshot)
        var totSrv = await _db.ReservaServiciosAdicionales.AsNoTracking()
            .Where(rs => reservaIds.Contains(rs.ReservaId))
            .GroupBy(rs => rs.ReservaId)
            .Select(g => new { ReservaId = g.Key, Total = g.Sum(x => x.Cantidad * x.PrecioUnitarioAplicado) })
            .ToListAsync(ct);

        var mapHab = totHab.ToDictionary(x => x.ReservaId, x => x.Total);
        var mapSrv = totSrv.ToDictionary(x => x.ReservaId, x => x.Total);

        return [.. reservaInfo.Select(r => new ReservaCostoTotalRow
        {
            ReservaId         = r.ReservaId,
            ClienteId         = r.ClienteId,
            ClienteNombre     = $"{r.NombreCliente} {r.ApellidoCliente}",
            FechaEntrada      = r.FechaEntrada,
            FechaSalida       = r.FechaSalida,
            TotalHabitaciones = mapHab.TryGetValue(r.ReservaId, out var th) ? th : 0m,
            TotalServicios    = mapSrv.TryGetValue(r.ReservaId, out var ts) ? ts : 0m,
        })];
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RF-16: Uso de servicios — ranking por ingresos
    // ─────────────────────────────────────────────────────────────────────────
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

        // Incluir NombreServicio en la clave de grupo para poder proyectarlo
        var rows = await query
            .GroupBy(x => new { x.s.ServicioAdicionalId, x.s.NombreServicio })
            .Select(g => new UsoServiciosRow
            {
                ServicioAdicionalId = g.Key.ServicioAdicionalId,
                ServicioNombre = g.Key.NombreServicio,
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
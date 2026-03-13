using SGRH.Application.Dtos.Reportes;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services.Time;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reportes;

public sealed class ListarReportesUseCase
{
    private readonly IReportesQueryService _reportes;
    private readonly ISystemClock _clock;

    public ListarReportesUseCase(
        IReportesQueryService reportes,
        ISystemClock clock)
    {
        _reportes = reportes;
        _clock = clock;
    }

    public async Task<ListarReportesResponse> ExecuteAsync(
        TipoReporte tipoReporte,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int? categoriaId = null,
        int? habitacionId = null,
        int? clienteId = null,
        int? servicioId = null,
        int? top = null,
        EstadoReserva? estado = null,
        CancellationToken ct = default)
    {
        var filtros = new ReportesQueryFiltros(
            Desde: fechaDesde,
            Hasta: fechaHasta,
            CategoriaHabitacionId: categoriaId,
            HabitacionId: habitacionId,
            ClienteId: clienteId,
            ServicioAdicionalId: servicioId,
            Top: top);

        List<ReporteFilaDto> filas = tipoReporte switch
        {
            TipoReporte.Ocupacion => await BuildOcupacionAsync(filtros, estado, ct),
            TipoReporte.Ingresos => await BuildIngresosAsync(filtros, estado, ct),
            TipoReporte.UsoServicios => await BuildUsoServiciosAsync(filtros, estado, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(tipoReporte))
        };

        var reporte = new ReporteDto(
            TipoReporte: tipoReporte.ToString(),
            Agrupacion: tipoReporte == TipoReporte.UsoServicios ? "Servicio" : "Periodo",
            FechaDesde: fechaDesde ?? DateTime.MinValue,
            FechaHasta: fechaHasta ?? DateTime.MaxValue,
            GeneradoEn: _clock.UtcNow,           // ← ISystemClock, nunca DateTime.UtcNow directo
            Filas: filas);

        return new ListarReportesResponse(reporte);
    }

    private async Task<List<ReporteFilaDto>> BuildOcupacionAsync(
        ReportesQueryFiltros filtros, EstadoReserva? estado, CancellationToken ct)
    {
        var rows = await _reportes.GetOcupacionActivaAsync(filtros, estado, ct);

        return rows
            .GroupBy(r => r.CategoriaNombre)
            .Select(g => new ReporteFilaDto(
                Etiqueta: g.Key,
                Valor: g.Count(),
                Detalle: $"{g.Count()} habitación(es) ocupada(s)"))
            .ToList();
    }

    private async Task<List<ReporteFilaDto>> BuildIngresosAsync(
        ReportesQueryFiltros filtros, EstadoReserva? estado, CancellationToken ct)
    {
        var rows = await _reportes.GetReservaCostoTotalAsync(filtros, estado, ct);

        return rows
            .GroupBy(r => r.FechaEntrada.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new ReporteFilaDto(
                Etiqueta: g.Key,
                Valor: g.Sum(r => r.TotalHabitaciones + r.TotalServicios),
                Detalle: $"{g.Count()} reserva(s)"))
            .ToList();
    }

    private async Task<List<ReporteFilaDto>> BuildUsoServiciosAsync(
        ReportesQueryFiltros filtros, EstadoReserva? estado, CancellationToken ct)
    {
        var rows = await _reportes.GetUsoServiciosAsync(filtros, estado, ct);

        return rows
            .Select(r => new ReporteFilaDto(
                Etiqueta: r.ServicioNombre,
                Valor: r.CantidadSolicitudes,
                Detalle: $"Ingresos: {r.IngresoTotal:C}"))
            .ToList();
    }
}
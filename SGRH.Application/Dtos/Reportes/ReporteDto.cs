using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Reportes;
public sealed record ReporteFilaDto(
    string Etiqueta,   // Ej: "2025-01", "Suite Presidencial", "Desayuno Buffet"
    decimal Valor,     // Ej: porcentaje ocupación, total ingresos, cantidad usos
    string? Detalle);  // Información adicional opcional

public sealed record ReporteDto(
    string TipoReporte,
    string Agrupacion,
    DateTime FechaDesde,
    DateTime FechaHasta,
    DateTime GeneradoEn,
    IReadOnlyList<ReporteFilaDto> Filas);


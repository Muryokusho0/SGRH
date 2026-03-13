using SGRH.Application.Dtos.Reportes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reportes;

public sealed record ListarReportesResponse(ReporteDto Reporte);

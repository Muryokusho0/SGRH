using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Enums;

/// <summary>
/// Define los niveles de agrupación temporal utilizados en los reportes y consultas estadísticas.
/// </summary>
public enum AgrupacionTemporal
{
    /// <summary>
    /// Agrupa los resultados por día.
    /// </summary>
    Diario,

    /// <summary>
    /// Agrupa los resultados por mes.
    /// </summary>
    Mensual,

    /// <summary>
    /// Agrupa los resultados por año.
    /// </summary>
    Anual
}
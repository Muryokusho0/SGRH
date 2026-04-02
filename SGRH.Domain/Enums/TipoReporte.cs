using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Enums;

/// <summary>
/// Indica el tipo de reporte que se desea generar o consultar en el sistema.
/// Se utiliza para seleccionar la lógica de consulta apropiada en el servicio de reportes.
/// </summary>
public enum TipoReporte
{
    /// <summary>
    /// Reporte de ocupación de habitaciones: muestra qué habitaciones están
    /// activas, por quién y en qué rango de fechas.
    /// </summary>
    Ocupacion,

    /// <summary>
    /// Reporte de ingresos: detalla el costo total de las reservas,
    /// desglosado por habitaciones y servicios adicionales.
    /// </summary>
    Ingresos,

    /// <summary>
    /// Reporte de uso de servicios adicionales: muestra la cantidad de solicitudes
    /// y el ingreso generado por cada servicio ofrecido.
    /// </summary>
    UsoServicios
}
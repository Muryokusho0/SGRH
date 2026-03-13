using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.CambiarEstadoHabitacion;
// EstadoHabitacion válidos: Disponible, Mantenimiento, Ocupado, Limpieza
// El motivo es opcional para Disponible, requerido para los demás estados.
public sealed record CambiarEstadoHabitacionRequest(
    int HabitacionId,
    string NuevoEstado,
    string? Motivo,
    AuditInfo AuditInfo);
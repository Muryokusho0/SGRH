using SGRH.Application.Dtos.Habitaciones;
using SGRH.Domain.Entities.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class HabitacionMapper
{
    // ── HabitacionHistorial → HabitacionHistorialDto ──────────────────────

    public static HabitacionHistorialDto ToDto(this HabitacionHistorial historial) =>
        new(
            HabitacionHistorialId: historial.HabitacionHistorialId,
            EstadoHabitacion: historial.EstadoHabitacion.ToString(),
            Motivo: historial.MotivoCambio,
            FechaInicio: historial.FechaInicio,
            FechaFin: historial.FechaFin);

    // ── Habitacion → HabitacionDto ────────────────────────────────────────
    public static HabitacionDto ToDto(
        this Habitacion habitacion,
        string nombreCategoria) =>
        new(
            HabitacionId: habitacion.HabitacionId,
            CategoriaHabitacionId: habitacion.CategoriaHabitacionId,
            NombreCategoria: nombreCategoria,
            NumeroHabitacion: habitacion.NumeroHabitacion,
            Piso: habitacion.Piso,
            EstadoActual: habitacion.EstadoActual?.EstadoHabitacion.ToString()
                                   ?? "Desconocido",
            Historial: habitacion.Historial
                                   .Select(h => h.ToDto())
                                   .ToList());

    public static IReadOnlyList<HabitacionDto> ToDtoList(
        this IEnumerable<Habitacion> habitaciones,
        // Diccionario CategoriaHabitacionId → NombreCategoria
        // para evitar N consultas adicionales al repositorio.
        IReadOnlyDictionary<int, string> nombresPorCategoria) =>
        habitaciones
            .Select(h => h.ToDto(
                nombresPorCategoria.TryGetValue(h.CategoriaHabitacionId, out var nombre)
                    ? nombre
                    : string.Empty))
            .ToList();
}

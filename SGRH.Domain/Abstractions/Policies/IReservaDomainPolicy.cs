using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Policies;

public interface IReservaDomainPolicy
{
    // Temporada por fecha de entrada (puede ser null si no hay temporada)
    int? GetTemporadaId(DateTime fechaEntrada);

    // Validar que una habitación está disponible para el rango y reserva actual
    void EnsureHabitacionDisponible(int habitacionId, DateTime fechaEntrada, DateTime fechaSalida, int? reservaId);

    // Validar que una habitación NO está en mantenimiento para el rango
    void EnsureHabitacionNoEnMantenimiento(int habitacionId, DateTime fechaEntrada, DateTime fechaSalida);

    // Obtener tarifa aplicable por habitación para la fecha (temp + categoría)
    decimal GetTarifaAplicada(int habitacionId, DateTime fechaEntrada);

    // Validar disponibilidad de un servicio para temporada (si temporada != null)
    void EnsureServicioDisponibleEnTemporada(int servicioAdicionalId, int? temporadaId);

    // Obtener precio unitario de servicio para una reserva (tu regla MAX por categoría)
    decimal GetPrecioServicioAplicado(int reservaId, int servicioAdicionalId);
}

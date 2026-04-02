using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Policies;

/// <summary>
/// Define las reglas de negocio que la entidad <c>Reserva</c> necesita consultar
/// para validar disponibilidad, calcular tarifas y verificar servicios.
/// La implementación reside en la capa de infraestructura o aplicación, nunca en el dominio.
/// </summary>
/// <remarks>
/// Este patrón permite que la entidad <c>Reserva</c> aplique reglas de negocio
/// que requieren datos de la base de datos sin acoplarse directamente a repositorios.
/// </remarks>
public interface IReservaDomainPolicy
{
    /// <summary>
    /// Obtiene el identificador de la temporada activa para la fecha de entrada indicada.
    /// </summary>
    /// <param name="fechaEntrada">Fecha de check-in de la reserva.</param>
    /// <returns>Id de la temporada activa, o <c>null</c> si no hay temporada para esa fecha.</returns>
    int? GetTemporadaId(DateTime fechaEntrada);

    /// <summary>
    /// Valida que una habitación está disponible para el rango de fechas indicado,
    /// excluyendo opcionalmente la reserva actual para evitar conflictos al editar.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación a verificar.</param>
    /// <param name="fechaEntrada">Fecha de inicio del rango.</param>
    /// <param name="fechaSalida">Fecha de fin del rango.</param>
    /// <param name="reservaId">Id de la reserva a excluir en la verificación (puede ser <c>null</c> para nuevas reservas).</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la habitación no está disponible.</exception>
    void EnsureHabitacionDisponible(int habitacionId, DateTime fechaEntrada, DateTime fechaSalida, int? reservaId);

    /// <summary>
    /// Valida que una habitación no esté en estado de mantenimiento durante el rango de fechas indicado.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación a verificar.</param>
    /// <param name="fechaEntrada">Fecha de inicio del rango.</param>
    /// <param name="fechaSalida">Fecha de fin del rango.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la habitación está en mantenimiento.</exception>
    void EnsureHabitacionNoEnMantenimiento(int habitacionId, DateTime fechaEntrada, DateTime fechaSalida);

    /// <summary>
    /// Obtiene la tarifa por noche aplicable para una habitación en la fecha de entrada indicada,
    /// considerando la temporada activa y la categoría de la habitación.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación.</param>
    /// <param name="fechaEntrada">Fecha de entrada para determinar la temporada y la tarifa.</param>
    /// <returns>Tarifa por noche aplicable en la moneda local.</returns>
    decimal GetTarifaAplicada(int habitacionId, DateTime fechaEntrada);

    /// <summary>
    /// Valida que un servicio adicional esté disponible para la temporada activa.
    /// Si no hay temporada activa (<paramref name="temporadaId"/> es <c>null</c>), la validación se omite.
    /// </summary>
    /// <param name="servicioAdicionalId">Id del servicio adicional a verificar.</param>
    /// <param name="temporadaId">Id de la temporada activa, o <c>null</c> si no hay temporada.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si el servicio no está disponible en la temporada.</exception>
    void EnsureServicioDisponibleEnTemporada(int servicioAdicionalId, int? temporadaId);

    /// <summary>
    /// Obtiene el precio unitario aplicado para un servicio adicional en el contexto de una reserva.
    /// Aplica la regla de negocio de precio máximo por categoría de habitación de la reserva.
    /// </summary>
    /// <param name="reservaId">Id de la reserva donde se contratará el servicio.</param>
    /// <param name="servicioAdicionalId">Id del servicio adicional.</param>
    /// <returns>Precio unitario del servicio en la moneda local.</returns>
    decimal GetPrecioServicioAplicado(int reservaId, int servicioAdicionalId);
}

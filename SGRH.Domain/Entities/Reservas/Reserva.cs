using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Reservas;

public sealed class Reserva : EntityBase
{
    public int ReservaId { get; private set; }
    public int ClienteId { get; private set; }
    public EstadoReserva EstadoReserva { get; private set; }
    public DateTime FechaReserva { get; private set; }
    public DateTime FechaEntrada { get; private set; }
    public DateTime FechaSalida { get; private set; }

    public decimal CostoTotal =>
        _habitaciones.Sum(h => h.TarifaAplicada) +
        _servicios.Sum(s => s.SubTotal);

    private readonly List<DetalleReserva> _habitaciones = [];
    public IReadOnlyCollection<DetalleReserva> Habitaciones => _habitaciones;

    private readonly List<ReservaServicioAdicional> _servicios = [];
    public IReadOnlyCollection<ReservaServicioAdicional> Servicios => _servicios;

    private Reserva() { }

    public Reserva(int clienteId, DateTime fechaEntrada, DateTime fechaSalida)
    {
        Guard.AgainstOutOfRange(clienteId, nameof(clienteId), 0);
        Guard.AgainstInvalidDateRange(fechaEntrada, fechaSalida,
                                      nameof(fechaEntrada), nameof(fechaSalida));

        ClienteId = clienteId;
        FechaEntrada = fechaEntrada;
        FechaSalida = fechaSalida;
        EstadoReserva = EstadoReserva.Pendiente;
        FechaReserva = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────
    // Guard interno
    // ─────────────────────────────────────────

    private void EnsureEditable()
    {
        if (EstadoReserva == EstadoReserva.Confirmada)
            throw new BusinessRuleViolationException(
                "Una reserva confirmada no puede ser modificada.");

        if (EstadoReserva == EstadoReserva.Cancelada)
            throw new BusinessRuleViolationException(
                "Una reserva cancelada no puede ser modificada.");

        if (EstadoReserva == EstadoReserva.Finalizada)
            throw new BusinessRuleViolationException(
                "Una reserva finalizada no puede ser modificada.");
    }

    // ─────────────────────────────────────────
    // Fechas
    // ─────────────────────────────────────────

    public void CambiarFechas(
        DateTime nuevaEntrada,
        DateTime nuevaSalida,
        IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();

        Guard.AgainstInvalidDateRange(nuevaEntrada, nuevaSalida,
                                      nameof(nuevaEntrada), nameof(nuevaSalida));

        foreach (var detalle in _habitaciones)
        {
            policy.EnsureHabitacionDisponible(
                detalle.HabitacionId, nuevaEntrada, nuevaSalida,
                ReservaId == 0 ? null : ReservaId);

            policy.EnsureHabitacionNoEnMantenimiento(
                detalle.HabitacionId, nuevaEntrada, nuevaSalida);
        }

        var temporadaId = policy.GetTemporadaId(nuevaEntrada);
        foreach (var servicio in _servicios)
            policy.EnsureServicioDisponibleEnTemporada(
                servicio.ServicioAdicionalId, temporadaId);

        FechaEntrada = nuevaEntrada;
        FechaSalida = nuevaSalida;

        RecalcularSnapshots(policy);
    }

    public void AgregarHabitacion(int habitacionId, IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();
        Guard.AgainstOutOfRange(habitacionId, nameof(habitacionId), 0);

        if (_habitaciones.Any(h => h.HabitacionId == habitacionId))
            throw new ConflictException(
                "La habitación ya está incluida en esta reserva.");

        policy.EnsureHabitacionDisponible(
            habitacionId, FechaEntrada, FechaSalida,
            ReservaId == 0 ? null : ReservaId);

        policy.EnsureHabitacionNoEnMantenimiento(
            habitacionId, FechaEntrada, FechaSalida);

        var tarifa = policy.GetTarifaAplicada(habitacionId, FechaEntrada);

        _habitaciones.Add(new DetalleReserva(ReservaId, habitacionId, tarifa));

        if (_servicios.Count > 0)
            RecalcularSnapshots(policy);
    }

    public void QuitarHabitacion(int habitacionId, IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();

        var detalle = _habitaciones.FirstOrDefault(h => h.HabitacionId == habitacionId)
            ?? throw new NotFoundException("DetalleReserva", habitacionId.ToString());

        _habitaciones.Remove(detalle);

        if (_servicios.Count > 0)
            RecalcularSnapshots(policy);
    }

    public void AgregarServicio(
        int servicioAdicionalId,
        int cantidad,
        IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();
        Guard.AgainstOutOfRange(servicioAdicionalId, nameof(servicioAdicionalId), 0);
        Guard.AgainstOutOfRange(cantidad, nameof(cantidad), 0);

        if (_habitaciones.Count == 0)
            throw new BusinessRuleViolationException(
                "Debe agregar al menos una habitación antes de agregar servicios.");

        if (_servicios.Any(s => s.ServicioAdicionalId == servicioAdicionalId))
            throw new ConflictException(
                "El servicio ya está en la reserva. Modifica la cantidad en su lugar.");

        var temporadaId = policy.GetTemporadaId(FechaEntrada);
        policy.EnsureServicioDisponibleEnTemporada(servicioAdicionalId, temporadaId);

        var precioUnitario = policy.GetPrecioServicioAplicado(
            ReservaId, servicioAdicionalId);

        _servicios.Add(new ReservaServicioAdicional(
            ReservaId, servicioAdicionalId, cantidad, precioUnitario));
    }

    public void CambiarCantidadServicio(int servicioAdicionalId, int nuevaCantidad)
    {
        EnsureEditable();

        var servicio = _servicios
            .FirstOrDefault(s => s.ServicioAdicionalId == servicioAdicionalId)
            ?? throw new NotFoundException(
                "ReservaServicioAdicional", servicioAdicionalId.ToString());

        servicio.CambiarCantidad(nuevaCantidad);
    }

    public void QuitarServicio(int servicioAdicionalId)
    {
        EnsureEditable();

        var servicio = _servicios
            .FirstOrDefault(s => s.ServicioAdicionalId == servicioAdicionalId)
            ?? throw new NotFoundException(
                "ReservaServicioAdicional", servicioAdicionalId.ToString());

        _servicios.Remove(servicio);
    }

    public void Confirmar()
    {
        if (EstadoReserva != EstadoReserva.Pendiente)
            throw new BusinessRuleViolationException(
                $"Solo una reserva Pendiente puede confirmarse. Estado actual: {EstadoReserva}.");

        if (_habitaciones.Count == 0)
            throw new BusinessRuleViolationException(
                "No se puede confirmar una reserva sin habitaciones.");

        EstadoReserva = EstadoReserva.Confirmada;
    }

    public void Cancelar()
    {
        if (EstadoReserva == EstadoReserva.Finalizada)
            throw new BusinessRuleViolationException(
                "Una reserva finalizada no puede cancelarse.");

        if (EstadoReserva == EstadoReserva.Cancelada)
            throw new BusinessRuleViolationException(
                "La reserva ya está cancelada.");

        EstadoReserva = EstadoReserva.Cancelada;
    }

    public void Finalizar()
    {
        if (EstadoReserva != EstadoReserva.Confirmada)
            throw new BusinessRuleViolationException(
                "Solo una reserva Confirmada puede finalizarse.");

        EstadoReserva = EstadoReserva.Finalizada;
    }

    private void RecalcularSnapshots(IReservaDomainPolicy policy)
    {
        if (EstadoReserva != EstadoReserva.Pendiente) return;

        foreach (var detalle in _habitaciones)
        {
            var nuevaTarifa = policy.GetTarifaAplicada(
                detalle.HabitacionId, FechaEntrada);
            detalle.ActualizarTarifa(nuevaTarifa);
        }

        var temporadaId = policy.GetTemporadaId(FechaEntrada);
        foreach (var servicio in _servicios)
        {
            policy.EnsureServicioDisponibleEnTemporada(
                servicio.ServicioAdicionalId, temporadaId);

            var nuevoPrecio = policy.GetPrecioServicioAplicado(
                ReservaId, servicio.ServicioAdicionalId);
            servicio.ActualizarPrecioUnitario(nuevoPrecio);
        }
    }

    protected override object GetKey() => ReservaId;
}

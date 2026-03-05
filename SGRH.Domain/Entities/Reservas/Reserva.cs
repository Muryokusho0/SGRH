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

    public EstadoReserva EstadoReserva { get; private set; } = EstadoReserva.Pendiente;

    public DateTime FechaReserva { get; private set; } = DateTime.UtcNow;
    public DateTime FechaEntrada { get; private set; }
    public DateTime FechaSalida { get; private set; }

    private readonly List<DetalleReserva> _habitaciones = [];
    public IReadOnlyCollection<DetalleReserva> Habitaciones => _habitaciones;

    private readonly List<ReservaServicioAdicional> _servicios = [];
    public IReadOnlyCollection<ReservaServicioAdicional> Servicios => _servicios;

    private Reserva() { }

    public Reserva(int clienteId, DateTime fechaEntrada, DateTime fechaSalida)
    {
        Guard.AgainstOutOfRange(clienteId, nameof(clienteId), 0);
        Guard.AgainstInvalidDateRange(fechaEntrada, fechaSalida, nameof(fechaEntrada), nameof(fechaSalida));

        ClienteId = clienteId;
        FechaEntrada = fechaEntrada;
        FechaSalida = fechaSalida;
        EstadoReserva = EstadoReserva.Pendiente;
        FechaReserva = DateTime.UtcNow;
    }

    // ---------------------------
    // Reglas de estado (inmutabilidad)
    // ---------------------------
    private void EnsureEditable()
    {
        if (EstadoReserva == EstadoReserva.Confirmada)
            throw new BusinessRuleViolationException("Reserva confirmada: no se permiten cambios (snapshot inmutable).");

        if (EstadoReserva == EstadoReserva.Cancelada)
            throw new BusinessRuleViolationException("Reserva cancelada: no se permiten cambios.");
    }

    // ---------------------------
    // Fechas
    // ---------------------------
    public void CambiarFechas(DateTime nuevaEntrada, DateTime nuevaSalida, IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();
        Guard.AgainstInvalidDateRange(nuevaEntrada, nuevaSalida, nameof(nuevaEntrada), nameof(nuevaSalida));

        // Validaciones de disponibilidad por habitación con las nuevas fechas
        foreach (var dr in _habitaciones)
        {
            policy.EnsureHabitacionDisponible(dr.HabitacionId, nuevaEntrada, nuevaSalida, ReservaId == 0 ? null : ReservaId);
            policy.EnsureHabitacionNoEnMantenimiento(dr.HabitacionId, nuevaEntrada, nuevaSalida);
        }

        // Validar servicios por temporada (si hay temporada)
        var temporadaId = policy.GetTemporadaId(nuevaEntrada);
        foreach (var s in _servicios)
            policy.EnsureServicioDisponibleEnTemporada(s.ServicioAdicionalId, temporadaId);

        // Aplicar cambios
        FechaEntrada = nuevaEntrada;
        FechaSalida = nuevaSalida;

        // Repricing automático (solo si sigue pendiente)
        RecalcularSnapshots(policy);
    }

    // ---------------------------
    // Habitaciones
    // ---------------------------
    public void AgregarHabitacion(int habitacionId, IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();
        Guard.AgainstOutOfRange(habitacionId, nameof(habitacionId), 0);

        if (_habitaciones.Any(x => x.HabitacionId == habitacionId))
            throw new BusinessRuleViolationException("La habitación ya está agregada a la reserva.");

        policy.EnsureHabitacionDisponible(habitacionId, FechaEntrada, FechaSalida, ReservaId == 0 ? null : ReservaId);
        policy.EnsureHabitacionNoEnMantenimiento(habitacionId, FechaEntrada, FechaSalida);

        var tarifa = policy.GetTarifaAplicada(habitacionId, FechaEntrada);
        var detalle = new DetalleReserva(ReservaId, habitacionId, tarifa);

        _habitaciones.Add(detalle);

        // Si ya hay servicios, sus precios dependen de categorías presentes (regla MAX),
        // así que recalcúlalo para mantener snapshot consistente (en Pendiente).
        if (_servicios.Count > 0)
            RecalcularSnapshots(policy);
    }

    public void QuitarHabitacion(int habitacionId, IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();

        var item = _habitaciones.FirstOrDefault(x => x.HabitacionId == habitacionId)
            ?? throw new BusinessRuleViolationException("La habitación no existe en la reserva.");
        _habitaciones.Remove(item);

        // Si quitas habitaciones, los servicios pueden cambiar de precio (MAX por categoría)
        if (_servicios.Count > 0)
            RecalcularSnapshots(policy);
    }

    // ---------------------------
    // Servicios
    // ---------------------------
    public void AgregarServicio(int servicioAdicionalId, int cantidad, IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();

        Guard.AgainstOutOfRange(servicioAdicionalId, nameof(servicioAdicionalId), 0);
        Guard.AgainstOutOfRange(cantidad, nameof(cantidad), 0);

        if (_habitaciones.Count == 0)
            throw new BusinessRuleViolationException("No se puede agregar un servicio sin habitaciones (se requiere para calcular precio por categoría).");

        if (_servicios.Any(x => x.ServicioAdicionalId == servicioAdicionalId))
            throw new BusinessRuleViolationException("El servicio ya existe en la reserva (actualiza la cantidad).");

        var temporadaId = policy.GetTemporadaId(FechaEntrada);
        policy.EnsureServicioDisponibleEnTemporada(servicioAdicionalId, temporadaId);

        // Precio unitario (regla de tu SQL: MAX precio por categorías presentes)
        var precioUnitario = policy.GetPrecioServicioAplicado(ReservaId, servicioAdicionalId);

        var rsa = new ReservaServicioAdicional(ReservaId, servicioAdicionalId, cantidad, precioUnitario);
        _servicios.Add(rsa);
    }

    public void CambiarCantidadServicio(int servicioAdicionalId, int nuevaCantidad)
    {
        EnsureEditable();
        Guard.AgainstOutOfRange(nuevaCantidad, nameof(nuevaCantidad), 0);

        var s = _servicios.FirstOrDefault(x => x.ServicioAdicionalId == servicioAdicionalId)
            ??throw new BusinessRuleViolationException("El servicio no existe en la reserva.");

        s.CambiarCantidad(nuevaCantidad);
    }

    public void QuitarServicio(int servicioAdicionalId)
    {
        EnsureEditable();

        var s = _servicios.FirstOrDefault(x => x.ServicioAdicionalId == servicioAdicionalId)
            ??throw new BusinessRuleViolationException("El servicio no existe en la reserva.");

        _servicios.Remove(s);
    }

    // ---------------------------
    // Confirmación / cancelación
    // ---------------------------
    public void Confirmar()
    {
        if (EstadoReserva == EstadoReserva.Cancelada)
            throw new BusinessRuleViolationException("No se puede confirmar una reserva cancelada.");

        if (_habitaciones.Count == 0)
            throw new BusinessRuleViolationException("No se puede confirmar una reserva sin habitaciones.");

        EstadoReserva = EstadoReserva.Confirmada;
    }

    public void Cancelar()
    {
        if (EstadoReserva == EstadoReserva.Confirmada)
        {
            // SQL permite cancelar confirmadas (no lo bloquea por definición),
        }

        EstadoReserva = EstadoReserva.Cancelada;
    }

    // ---------------------------
    // Repricing (solo Pendiente)
    // ---------------------------
    private void RecalcularSnapshots(IReservaDomainPolicy policy)
    {
        if (EstadoReserva != EstadoReserva.Pendiente)
            return;

        // Recalcular tarifas habitaciones (por temporada/fecha entrada)
        foreach (var dr in _habitaciones)
        {
            var nueva = policy.GetTarifaAplicada(dr.HabitacionId, FechaEntrada);
            dr.ActualizarTarifa(nueva);
        }

        // Validar disponibilidad de servicios por temporada y recalcular precio unitario
        var temporadaId = policy.GetTemporadaId(FechaEntrada);
        foreach (var s in _servicios)
        {
            policy.EnsureServicioDisponibleEnTemporada(s.ServicioAdicionalId, temporadaId);
            var nuevoPrecio = policy.GetPrecioServicioAplicado(ReservaId, s.ServicioAdicionalId);
            s.ActualizarPrecioUnitario(nuevoPrecio);
        }
    }

    protected override object GetKey() => ReservaId;
}

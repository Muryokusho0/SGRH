using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Reservas;

/// <summary>
/// Raíz de agregado que representa una reserva de hotel.
/// Gestiona el ciclo de vida completo de la reserva, incluyendo habitaciones,
/// servicios adicionales, fechas y estado, aplicando todas las reglas de negocio del dominio.
/// </summary>
public sealed class Reserva : EntityBase
{
    /// <summary>
    /// Identificador único de la reserva.
    /// </summary>
    public int ReservaId { get; private set; }

    /// <summary>
    /// Identificador del cliente que realizó la reserva.
    /// </summary>
    public int ClienteId { get; private set; }

    /// <summary>
    /// Estado actual en el ciclo de vida de la reserva.
    /// </summary>
    public EstadoReserva EstadoReserva { get; private set; }

    /// <summary>
    /// Fecha y hora (hora local UTC-4) en que se creó la reserva.
    /// </summary>
    public DateTime FechaReserva { get; private set; }

    /// <summary>
    /// Fecha de entrada (check-in) del huésped.
    /// </summary>
    public DateTime FechaEntrada { get; private set; }

    /// <summary>
    /// Fecha de salida (check-out) del huésped.
    /// </summary>
    public DateTime FechaSalida { get; private set; }

    /// <summary>
    /// Costo total calculado de la reserva, sumando las tarifas de todas las habitaciones
    /// y los subtotales de todos los servicios adicionales.
    /// </summary>
    public decimal CostoTotal =>
        _habitaciones.Sum(h => h.TarifaAplicada) +
        _servicios.Sum(s => s.SubTotal);

    private readonly List<DetalleReserva> _habitaciones = [];

    /// <summary>
    /// Colección de habitaciones incluidas en la reserva con sus tarifas congeladas.
    /// </summary>
    public IReadOnlyCollection<DetalleReserva> Habitaciones => _habitaciones;

    private readonly List<ReservaServicioAdicional> _servicios = [];

    /// <summary>
    /// Colección de servicios adicionales contratados en la reserva.
    /// </summary>
    public IReadOnlyCollection<ReservaServicioAdicional> Servicios => _servicios;

    private Reserva() { }

    /// <summary>
    /// Crea una nueva reserva en estado <see cref="EstadoReserva.Pendiente"/>
    /// para el cliente y rango de fechas indicados.
    /// </summary>
    /// <param name="clienteId">Id del cliente que realiza la reserva (mayor a 0).</param>
    /// <param name="fechaEntrada">Fecha de check-in (debe ser anterior a <paramref name="fechaSalida"/>).</param>
    /// <param name="fechaSalida">Fecha de check-out (debe ser posterior a <paramref name="fechaEntrada"/>).</param>
    public Reserva(int clienteId, DateTime fechaEntrada, DateTime fechaSalida)
    {
        Guard.AgainstOutOfRange(clienteId, nameof(clienteId), 0);
        Guard.AgainstInvalidDateRange(fechaEntrada, fechaSalida,
            nameof(fechaEntrada), nameof(fechaSalida));

        ClienteId = clienteId;
        FechaEntrada = fechaEntrada;
        FechaSalida = fechaSalida;
        EstadoReserva = EstadoReserva.Pendiente;
        FechaReserva = HoraLocal.Ahora;
    }

    // ─────────────────────────────────────────────────────────────
    // Guard interno
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica que la reserva esté en un estado editable (solo <see cref="EstadoReserva.Pendiente"/>).
    /// Lanza una excepción si la reserva ya fue confirmada, cancelada o finalizada.
    /// </summary>
    /// <exception cref="Exceptions.BusinessRuleViolationException">
    /// Se lanza si la reserva no está en estado <see cref="EstadoReserva.Pendiente"/>.
    /// </exception>
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

    // ─────────────────────────────────────────────────────────────
    // Fechas
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Cambia las fechas de entrada y salida de la reserva, validando disponibilidad
    /// de habitaciones y servicios para el nuevo rango mediante la política del dominio.
    /// </summary>
    /// <remarks>
    /// Los snapshots de tarifa y precio NO se recalculan aquí; esa responsabilidad
    /// recae en el trigger <c>TR_Reserva_CambioFechas_RevalidarHabitaciones</c> de la base de datos.
    /// </remarks>
    /// <param name="nuevaEntrada">Nueva fecha de check-in.</param>
    /// <param name="nuevaSalida">Nueva fecha de check-out.</param>
    /// <param name="policy">Política de dominio para validar disponibilidad y tarifas.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la reserva no es editable.</exception>
    /// <exception cref="Exceptions.ValidationException">Si el rango de fechas es inválido.</exception>
    public void CambiarFechas(
        DateTime nuevaEntrada,
        DateTime nuevaSalida,
        IReservaDomainPolicy policy)
    {
        Guard.AgainstNull(policy, nameof(policy));
        EnsureEditable();
        Guard.AgainstInvalidDateRange(nuevaEntrada, nuevaSalida,
            nameof(nuevaEntrada), nameof(nuevaSalida));

        // Validar disponibilidad de habitaciones en el nuevo rango
        foreach (var detalle in _habitaciones)
        {
            policy.EnsureHabitacionDisponible(
                detalle.HabitacionId, nuevaEntrada, nuevaSalida,
                ReservaId == 0 ? null : ReservaId);

            policy.EnsureHabitacionNoEnMantenimiento(
                detalle.HabitacionId, nuevaEntrada, nuevaSalida);
        }

        // Validar disponibilidad de servicios en la nueva temporada
        var temporadaId = policy.GetTemporadaId(nuevaEntrada);
        foreach (var servicio in _servicios)
            policy.EnsureServicioDisponibleEnTemporada(
                servicio.ServicioAdicionalId, temporadaId);

        FechaEntrada = nuevaEntrada;
        FechaSalida = nuevaSalida;

        // NO se llama RecalcularSnapshots aquí.
        // El trigger TR_Reserva_CambioFechas_RevalidarHabitaciones en la BD
        // recalcula automáticamente TarifaAplicada y PrecioUnitarioAplicado
        // cuando cambian FechaEntrada/FechaSalida en la tabla Reserva.
        // Hacerlo también desde EF causaría UPDATEs con OUTPUT en tablas
        // que tienen triggers, lo que SQL Server rechaza.
    }

    // ─────────────────────────────────────────────────────────────
    // Habitaciones
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Agrega una habitación a la reserva, validando disponibilidad y generando el snapshot de tarifa.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación a agregar (mayor a 0).</param>
    /// <param name="policy">Política de dominio para validar disponibilidad y obtener la tarifa aplicada.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la reserva no es editable.</exception>
    /// <exception cref="Exceptions.ConflictException">Si la habitación ya está en la reserva.</exception>
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

    /// <summary>
    /// Elimina una habitación de la reserva y recalcula los snapshots de servicios si es necesario.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación a quitar.</param>
    /// <param name="policy">Política de dominio utilizada para recalcular precios si quedan servicios.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la reserva no es editable.</exception>
    /// <exception cref="Exceptions.NotFoundException">Si la habitación no está en la reserva.</exception>
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

    // ─────────────────────────────────────────────────────────────
    // Servicios
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Agrega un servicio adicional a la reserva con la cantidad indicada y el precio congelado.
    /// Requiere que exista al menos una habitación en la reserva.
    /// </summary>
    /// <param name="servicioAdicionalId">Id del servicio adicional a agregar (mayor a 0).</param>
    /// <param name="cantidad">Cantidad de unidades del servicio (mayor a 0).</param>
    /// <param name="policy">Política de dominio para validar disponibilidad en temporada y obtener el precio.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la reserva no es editable o no tiene habitaciones.</exception>
    /// <exception cref="Exceptions.ConflictException">Si el servicio ya está en la reserva.</exception>
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

        var precioUnitario = policy.GetPrecioServicioAplicado(ReservaId, servicioAdicionalId);
        _servicios.Add(new ReservaServicioAdicional(
            ReservaId, servicioAdicionalId, cantidad, precioUnitario));
    }

    /// <summary>
    /// Elimina un servicio adicional de la reserva.
    /// </summary>
    /// <param name="servicioAdicionalId">Id del servicio adicional a quitar.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la reserva no es editable.</exception>
    /// <exception cref="Exceptions.NotFoundException">Si el servicio no está en la reserva.</exception>
    public void QuitarServicio(int servicioAdicionalId)
    {
        EnsureEditable();

        var servicio = _servicios.FirstOrDefault(s => s.ServicioAdicionalId == servicioAdicionalId)
            ?? throw new NotFoundException(
                "ReservaServicioAdicional", servicioAdicionalId.ToString());

        _servicios.Remove(servicio);
    }

    /// <summary>
    /// Modifica la cantidad de un servicio adicional ya incluido en la reserva.
    /// </summary>
    /// <param name="servicioAdicionalId">Id del servicio adicional a modificar.</param>
    /// <param name="nuevaCantidad">Nueva cantidad de unidades (mayor a 0).</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la reserva no es editable.</exception>
    /// <exception cref="Exceptions.NotFoundException">Si el servicio no está en la reserva.</exception>
    public void CambiarCantidadServicio(int servicioAdicionalId, int nuevaCantidad)
    {
        EnsureEditable();

        var servicio = _servicios.FirstOrDefault(s => s.ServicioAdicionalId == servicioAdicionalId)
            ?? throw new NotFoundException(
                "ReservaServicioAdicional", servicioAdicionalId.ToString());

        servicio.CambiarCantidad(nuevaCantidad);
    }

    // ─────────────────────────────────────────────────────────────
    // Estado
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Confirma la reserva, transicionando su estado de <see cref="EstadoReserva.Pendiente"/>
    /// a <see cref="EstadoReserva.Confirmada"/>. Requiere al menos una habitación.
    /// </summary>
    /// <exception cref="Exceptions.BusinessRuleViolationException">
    /// Si la reserva no está en estado Pendiente o no tiene habitaciones.
    /// </exception>
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

    /// <summary>
    /// Cancela la reserva, transicionando su estado a <see cref="EstadoReserva.Cancelada"/>.
    /// No puede cancelarse si ya está Finalizada o ya está Cancelada.
    /// </summary>
    /// <exception cref="Exceptions.BusinessRuleViolationException">
    /// Si la reserva ya está Finalizada o Cancelada.
    /// </exception>
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

    /// <summary>
    /// Finaliza la reserva, transicionando su estado de <see cref="EstadoReserva.Confirmada"/>
    /// a <see cref="EstadoReserva.Finalizada"/>. Es un estado terminal.
    /// </summary>
    /// <exception cref="Exceptions.BusinessRuleViolationException">
    /// Si la reserva no está en estado Confirmada.
    /// </exception>
    public void Finalizar()
    {
        if (EstadoReserva != EstadoReserva.Confirmada)
            throw new BusinessRuleViolationException(
                "Solo una reserva Confirmada puede finalizarse.");

        EstadoReserva = EstadoReserva.Finalizada;
    }

    // ─────────────────────────────────────────────────────────────
    // Privado
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Recalcula los snapshots de tarifa de habitaciones y precio de servicios
    /// cuando cambia la composición de habitaciones de la reserva.
    /// Solo aplica si la reserva está en estado Pendiente.
    /// </summary>
    /// <remarks>
    /// No debe llamarse desde <c>CambiarFechas</c>; ese recálculo lo realiza
    /// el trigger <c>TR_Reserva_CambioFechas_RevalidarHabitaciones</c> en la base de datos.
    /// </remarks>
    /// <param name="policy">Política de dominio para obtener tarifas y precios actualizados.</param>
    private void RecalcularSnapshots(IReservaDomainPolicy policy)
    {
        if (EstadoReserva != EstadoReserva.Pendiente) return;

        foreach (var detalle in _habitaciones)
        {
            var nuevaTarifa = policy.GetTarifaAplicada(detalle.HabitacionId, FechaEntrada);
            detalle.ActualizarTarifa(nuevaTarifa);
        }

        var temporadaId = policy.GetTemporadaId(FechaEntrada);
        foreach (var servicio in _servicios)
        {
            policy.EnsureServicioDisponibleEnTemporada(servicio.ServicioAdicionalId, temporadaId);
            var nuevoPrecio = policy.GetPrecioServicioAplicado(ReservaId, servicio.ServicioAdicionalId);
            servicio.ActualizarPrecioUnitario(nuevoPrecio);
        }
    }

    protected override object GetKey() => ReservaId;
}
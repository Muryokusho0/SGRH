using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Infrastructure.Services;

public sealed class ReservaDomainPolicy : IReservaDomainPolicy
{
    private readonly ITemporadaRepository _temporadas;
    private readonly IHabitacionRepository _habitaciones;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly ITarifaTemporadaRepository _tarifas;
    private readonly IReservaRepository _reservas;
    private readonly IServicioAdicionalRepository _servicios;
    private readonly IServicioCategoriaPrecioRepository _servicioPrecios;

    public ReservaDomainPolicy(
        ITemporadaRepository temporadas,
        IHabitacionRepository habitaciones,
        ICategoriaHabitacionRepository categorias,
        ITarifaTemporadaRepository tarifas,
        IReservaRepository reservas,
        IServicioAdicionalRepository servicios,
        IServicioCategoriaPrecioRepository servicioPrecios)
    {
        _temporadas = temporadas;
        _habitaciones = habitaciones;
        _categorias = categorias;
        _tarifas = tarifas;
        _reservas = reservas;
        _servicios = servicios;
        _servicioPrecios = servicioPrecios;
    }

    public int? GetTemporadaId(DateTime fechaEntrada)
        => _temporadas
            .GetByFechaAsync(fechaEntrada)
            .GetAwaiter().GetResult()
            ?.TemporadaId;

    public void EnsureHabitacionDisponible(
        int habitacionId, DateTime fechaEntrada, DateTime fechaSalida, int? reservaId)
    {
        var ocupada = _reservas
            .HabitacionTieneReservaActivaAsync(
                habitacionId, fechaEntrada, fechaSalida, reservaId)
            .GetAwaiter().GetResult();

        if (ocupada)
            throw new ConflictException(
                $"La habitación {habitacionId} no está disponible " +
                $"para el rango {fechaEntrada:d} – {fechaSalida:d}.");
    }

    public void EnsureHabitacionNoEnMantenimiento(
        int habitacionId, DateTime fechaEntrada, DateTime fechaSalida)
    {
        var habitacion = _habitaciones
            .GetByIdWithHistorialAsync(habitacionId)
            .GetAwaiter().GetResult()
            ?? throw new NotFoundException("Habitacion", habitacionId.ToString());

        if (habitacion.EstadoActual?.EstadoHabitacion == EstadoHabitacion.Mantenimiento)
            throw new BusinessRuleViolationException(
                $"La habitación {habitacionId} está en mantenimiento " +
                $"y no puede ser reservada.");
    }

    public decimal GetTarifaAplicada(int habitacionId, DateTime fechaEntrada)
    {
        var habitacion = _habitaciones
            .GetByIdAsync(habitacionId)
            .GetAwaiter().GetResult()
            ?? throw new NotFoundException("Habitacion", habitacionId.ToString());

        var temporadaId = GetTemporadaId(fechaEntrada);

        if (temporadaId.HasValue)
        {
            var tarifaTemporada = _tarifas
                .GetTarifaAsync(habitacion.CategoriaHabitacionId, temporadaId.Value)
                .GetAwaiter().GetResult();

            if (tarifaTemporada is not null)
                return tarifaTemporada.Precio;
        }

        var categoria = _categorias
            .GetByIdAsync(habitacion.CategoriaHabitacionId)
            .GetAwaiter().GetResult()
            ?? throw new NotFoundException(
                "CategoriaHabitacion", habitacion.CategoriaHabitacionId.ToString());

        return categoria.PrecioBase;
    }

    public void EnsureServicioDisponibleEnTemporada(
        int servicioAdicionalId, int? temporadaId)
    {
        if (!temporadaId.HasValue) return;

        var servicio = _servicios
            .GetByIdWithTemporadasAsync(servicioAdicionalId)
            .GetAwaiter().GetResult()
            ?? throw new NotFoundException("ServicioAdicional", servicioAdicionalId.ToString());

        // Si aplica a todas las temporadas, no hay nada que validar
        if (servicio.AplicaTodasTemporadas) return;

        if (!servicio.TemporadaIds.Contains(temporadaId.Value))
            throw new BusinessRuleViolationException(
                $"El servicio '{servicio.NombreServicio}' no está disponible " +
                $"para la temporada activa en las fechas de la reserva.");
    }

    public decimal GetPrecioServicioAplicado(int reservaId, int servicioAdicionalId)
    {
        var reserva = _reservas
            .GetByIdWithDetallesAsync(reservaId)
            .GetAwaiter().GetResult()
            ?? throw new NotFoundException("Reserva", reservaId.ToString());

        decimal? maxPrecio = null;

        foreach (var detalle in reserva.Habitaciones)
        {
            var habitacion = _habitaciones
                .GetByIdAsync(detalle.HabitacionId)
                .GetAwaiter().GetResult();

            if (habitacion is null) continue;

            var precio = _servicioPrecios
                .GetPrecioAsync(servicioAdicionalId, habitacion.CategoriaHabitacionId)
                .GetAwaiter().GetResult();

            if (precio.HasValue && (maxPrecio is null || precio.Value > maxPrecio.Value))
                maxPrecio = precio;
        }

        return maxPrecio ?? throw new BusinessRuleViolationException(
            $"No existe precio del servicio {servicioAdicionalId} " +
            $"para ninguna categoría incluida en la reserva {reservaId}.");
    }
}
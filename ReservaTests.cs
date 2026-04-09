using Moq;
using Xunit;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Entities;

/// <summary>
/// Pruebas unitarias de la entidad Reserva — el agregado más complejo del sistema.
/// Cubre todas las reglas de negocio: ciclo de vida de estados, habitaciones y servicios.
/// No toca base de datos ni API. IReservaDomainPolicy se mockea con Moq.
/// </summary>
public sealed class ReservaTests
{
    // ── Fixtures ─────────────────────────────────────────────────────────────

    private static readonly DateTime Hoy = DateTime.Today;
    private static readonly DateTime Manana = Hoy.AddDays(1);
    private static readonly DateTime PasadoManana = Hoy.AddDays(3);

    /// <summary>
    /// Crea una política mockeada que siempre aprueba sin restricciones.
    /// Cada test puede sobreescribir el comportamiento que necesita probar.
    /// </summary>
    private static Mock<IReservaDomainPolicy> PolicyQueAprueba(
        decimal tarifa = 1000m, decimal precioServicio = 200m,
        int? temporadaId = null)
    {
        var mock = new Mock<IReservaDomainPolicy>();

        // Habitación disponible sin restricciones
        mock.Setup(p => p.EnsureHabitacionDisponible(
                It.IsAny<int>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<int?>()))
            .Verifiable();

        // Habitación no está en mantenimiento
        mock.Setup(p => p.EnsureHabitacionNoEnMantenimiento(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Verifiable();

        // Tarifa configurada
        mock.Setup(p => p.GetTarifaAplicada(It.IsAny<int>(), It.IsAny<DateTime>()))
            .Returns(tarifa);

        // Servicio disponible en la temporada
        mock.Setup(p => p.EnsureServicioDisponibleEnTemporada(
                It.IsAny<int>(), It.IsAny<int?>()))
            .Verifiable();

        // Precio del servicio
        mock.Setup(p => p.GetPrecioServicioAplicado(
                It.IsAny<int>(), It.IsAny<int?>()))
            .Returns(precioServicio);

        // Temporada activa
        mock.Setup(p => p.GetTemporadaId(It.IsAny<DateTime>()))
            .Returns(temporadaId);

        return mock;
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DatosValidos_CreaReservaEnEstadoPendiente()
    {
        var reserva = new Reserva(clienteId: 1, Manana, PasadoManana);

        Assert.Equal(EstadoReserva.Pendiente, reserva.EstadoReserva);
        Assert.Equal(1, reserva.ClienteId);
        Assert.Equal(Manana, reserva.FechaEntrada);
        Assert.Equal(PasadoManana, reserva.FechaSalida);
        Assert.Empty(reserva.Habitaciones);
        Assert.Empty(reserva.Servicios);
    }

    [Fact]
    public void Constructor_ClienteIdCero_LanzaValidationException()
    {
        Assert.Throws<ValidationException>(
            () => new Reserva(clienteId: 0, Manana, PasadoManana));
    }

    [Fact]
    public void Constructor_FechaEntradaIgualSalida_LanzaValidationException()
    {
        Assert.Throws<ValidationException>(
            () => new Reserva(clienteId: 1, Manana, Manana));
    }

    [Fact]
    public void Constructor_FechaEntradaPosteriorSalida_LanzaValidationException()
    {
        Assert.Throws<ValidationException>(
            () => new Reserva(clienteId: 1, PasadoManana, Manana));
    }

    // ── AgregarHabitacion ─────────────────────────────────────────────────────

    [Fact]
    public void AgregarHabitacion_ReservaPendiente_AgregaCorrectamente()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba(tarifa: 1500m);

        reserva.AgregarHabitacion(habitacionId: 10, policy.Object);

        Assert.Single(reserva.Habitaciones);
        Assert.Equal(10, reserva.Habitaciones.First().HabitacionId);
        Assert.Equal(1500m, reserva.Habitaciones.First().TarifaAplicada);
    }

    [Fact]
    public void AgregarHabitacion_HabitacionDuplicada_LanzaConflictException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();

        reserva.AgregarHabitacion(10, policy.Object);

        Assert.Throws<ConflictException>(
            () => reserva.AgregarHabitacion(10, policy.Object));
    }

    [Fact]
    public void AgregarHabitacion_ReservaConfirmada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();

        Assert.Throws<BusinessRuleViolationException>(
            () => reserva.AgregarHabitacion(11, policy.Object));
    }

    [Fact]
    public void AgregarHabitacion_ReservaCancelada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();
        reserva.Cancelar();

        Assert.Throws<BusinessRuleViolationException>(
            () => reserva.AgregarHabitacion(11, policy.Object));
    }

    [Fact]
    public void AgregarHabitacion_HabitacionNoDisponible_LanzaConflictException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = new Mock<IReservaDomainPolicy>();

        policy.Setup(p => p.EnsureHabitacionDisponible(
                It.IsAny<int>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<int?>()))
            .Throws(new ConflictException("Habitación no disponible."));

        Assert.Throws<ConflictException>(
            () => reserva.AgregarHabitacion(10, policy.Object));
    }

    // ── QuitarHabitacion ──────────────────────────────────────────────────────

    [Fact]
    public void QuitarHabitacion_HabitacionExistente_LaElimina()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);

        reserva.QuitarHabitacion(10, policy.Object);

        Assert.Empty(reserva.Habitaciones);
    }

    [Fact]
    public void QuitarHabitacion_HabitacionNoExistente_LanzaNotFoundException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();

        Assert.Throws<NotFoundException>(
            () => reserva.QuitarHabitacion(99, policy.Object));
    }

    [Fact]
    public void QuitarHabitacion_ReservaConfirmada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();

        Assert.Throws<BusinessRuleViolationException>(
            () => reserva.QuitarHabitacion(10, policy.Object));
    }

    // ── AgregarServicio ───────────────────────────────────────────────────────

    [Fact]
    public void AgregarServicio_ConHabitacion_AgregaCorrectamente()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba(precioServicio: 300m);
        reserva.AgregarHabitacion(10, policy.Object);

        reserva.AgregarServicio(servicioAdicionalId: 5, cantidad: 2, policy.Object);

        Assert.Single(reserva.Servicios);
        Assert.Equal(5, reserva.Servicios.First().ServicioAdicionalId);
        Assert.Equal(2, reserva.Servicios.First().Cantidad);
        Assert.Equal(300m, reserva.Servicios.First().PrecioUnitarioAplicado);
        Assert.Equal(600m, reserva.Servicios.First().SubTotal);
    }

    [Fact]
    public void AgregarServicio_SinHabitaciones_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();

        // Sin habitaciones no se pueden agregar servicios
        Assert.Throws<BusinessRuleViolationException>(
            () => reserva.AgregarServicio(5, 1, policy.Object));
    }

    [Fact]
    public void AgregarServicio_ServicioDuplicado_LanzaConflictException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.AgregarServicio(5, 1, policy.Object);

        Assert.Throws<ConflictException>(
            () => reserva.AgregarServicio(5, 1, policy.Object));
    }

    // ── QuitarServicio ────────────────────────────────────────────────────────

    [Fact]
    public void QuitarServicio_ServicioExistente_LoElimina()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.AgregarServicio(5, 1, policy.Object);

        reserva.QuitarServicio(5);

        Assert.Empty(reserva.Servicios);
    }

    [Fact]
    public void QuitarServicio_ServicioNoExistente_LanzaNotFoundException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);

        Assert.Throws<NotFoundException>(() => reserva.QuitarServicio(99));
    }

    // ── Confirmar ─────────────────────────────────────────────────────────────

    [Fact]
    public void Confirmar_ReservaPendienteConHabitacion_CambiaEstadoAConfirmada()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);

        reserva.Confirmar();

        Assert.Equal(EstadoReserva.Confirmada, reserva.EstadoReserva);
    }

    [Fact]
    public void Confirmar_SinHabitaciones_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => reserva.Confirmar());
        Assert.Contains("habitaciones", ex.Message.ToLower());
    }

    [Fact]
    public void Confirmar_ReservaYaConfirmada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();

        Assert.Throws<BusinessRuleViolationException>(() => reserva.Confirmar());
    }

    // ── Cancelar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Cancelar_ReservaPendiente_CambiaEstadoACancelada()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);

        reserva.Cancelar();

        Assert.Equal(EstadoReserva.Cancelada, reserva.EstadoReserva);
    }

    [Fact]
    public void Cancelar_ReservaConfirmada_CambiaEstadoACancelada()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();

        reserva.Cancelar();

        Assert.Equal(EstadoReserva.Cancelada, reserva.EstadoReserva);
    }

    [Fact]
    public void Cancelar_ReservaFinalizada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();
        reserva.Finalizar();

        Assert.Throws<BusinessRuleViolationException>(() => reserva.Cancelar());
    }

    [Fact]
    public void Cancelar_ReservaYaCancelada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        reserva.Cancelar();

        Assert.Throws<BusinessRuleViolationException>(() => reserva.Cancelar());
    }

    // ── CostoTotal ────────────────────────────────────────────────────────────

    [Fact]
    public void CostoTotal_DosHabitacionesUnServicio_CalculaCorrectamente()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba(tarifa: 1000m, precioServicio: 500m);

        reserva.AgregarHabitacion(10, policy.Object);
        reserva.AgregarHabitacion(11, policy.Object);
        reserva.AgregarServicio(5, 2, policy.Object); // 500 * 2 = 1000

        // TarifaAplicada snapshot: 1000 + 1000 = 2000
        // SubTotal servicio: 500 * 2 = 1000
        // Total: 3000
        Assert.Equal(3000m, reserva.CostoTotal);
    }

    [Fact]
    public void CostoTotal_SinHabitacionesNiServicios_EsCero()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        Assert.Equal(0m, reserva.CostoTotal);
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Entities;

[TestClass]
public sealed class ReservaTests
{
    private static readonly DateTime Manana = DateTime.Today.AddDays(1);
    private static readonly DateTime PasadoManana = DateTime.Today.AddDays(3);

    private static Mock<IReservaDomainPolicy> PolicyQueAprueba(
        decimal tarifa = 1000m, decimal precioServicio = 200m,
        int? temporadaId = null)
    {
        var mock = new Mock<IReservaDomainPolicy>();
        mock.Setup(p => p.EnsureHabitacionDisponible(
            It.IsAny<int>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<int?>()));
        mock.Setup(p => p.EnsureHabitacionNoEnMantenimiento(
            It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()));
        mock.Setup(p => p.GetTarifaAplicada(
            It.IsAny<int>(), It.IsAny<DateTime>())).Returns(tarifa);
        mock.Setup(p => p.EnsureServicioDisponibleEnTemporada(
            It.IsAny<int>(), It.IsAny<int?>()));
        mock.Setup(p => p.GetPrecioServicioAplicado(
            It.IsAny<int>(), It.IsAny<int>())).Returns(precioServicio);
        mock.Setup(p => p.GetTemporadaId(
            It.IsAny<DateTime>())).Returns(temporadaId);
        return mock;
    }

    // -- Constructor -----------------------------------------------------------

    [TestMethod]
    public void Constructor_DatosValidos_CreaReservaEnEstadoPendiente()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        Assert.AreEqual(EstadoReserva.Pendiente, reserva.EstadoReserva);
        Assert.AreEqual(1, reserva.ClienteId);
        Assert.IsEmpty(reserva.Habitaciones);
        Assert.IsEmpty(reserva.Servicios);
    }

    [TestMethod]
    public void Constructor_ClienteIdCero_LanzaValidationException()
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Reserva(0, Manana, PasadoManana));
    }

    [TestMethod]
    public void Constructor_FechaEntradaIgualSalida_LanzaValidationException()
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Reserva(1, Manana, Manana));
    }

    [TestMethod]
    public void Constructor_FechaEntradaPosteriorSalida_LanzaValidationException()
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Reserva(1, PasadoManana, Manana));
    }

    // -- AgregarHabitacion -----------------------------------------------------

    [TestMethod]
    public void AgregarHabitacion_ReservaPendiente_AgregaCorrectamente()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba(tarifa: 1500m);

        reserva.AgregarHabitacion(10, policy.Object);

        Assert.HasCount(1, reserva.Habitaciones);
        Assert.AreEqual(10, reserva.Habitaciones.First().HabitacionId);
        Assert.AreEqual(1500m, reserva.Habitaciones.First().TarifaAplicada);
    }

    [TestMethod]
    public void AgregarHabitacion_HabitacionDuplicada_LanzaConflictException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);

        Assert.ThrowsExactly<ConflictException>(
            () => reserva.AgregarHabitacion(10, policy.Object));
    }

    [TestMethod]
    public void AgregarHabitacion_ReservaConfirmada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();

        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.AgregarHabitacion(11, policy.Object));
    }

    [TestMethod]
    public void AgregarHabitacion_ReservaCancelada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();
        reserva.Cancelar();

        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.AgregarHabitacion(11, policy.Object));
    }

    // -- QuitarHabitacion ------------------------------------------------------

    [TestMethod]
    public void QuitarHabitacion_HabitacionExistente_LaElimina()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.QuitarHabitacion(10, policy.Object);

        Assert.IsEmpty(reserva.Habitaciones);
    }

    [TestMethod]
    public void QuitarHabitacion_HabitacionNoExistente_LanzaNotFoundException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();

        Assert.ThrowsExactly<NotFoundException>(
            () => reserva.QuitarHabitacion(99, policy.Object));
    }

    [TestMethod]
    public void QuitarHabitacion_ReservaConfirmada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();

        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.QuitarHabitacion(10, policy.Object));
    }

    // -- AgregarServicio -------------------------------------------------------

    [TestMethod]
    public void AgregarServicio_ConHabitacion_AgregaCorrectamente()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba(precioServicio: 300m);
        reserva.AgregarHabitacion(10, policy.Object);

        reserva.AgregarServicio(5, 2, policy.Object);

        Assert.HasCount(1, reserva.Servicios);
        Assert.AreEqual(5, reserva.Servicios.First().ServicioAdicionalId);
        Assert.AreEqual(2, reserva.Servicios.First().Cantidad);
        Assert.AreEqual(300m, reserva.Servicios.First().PrecioUnitarioAplicado);
        Assert.AreEqual(600m, reserva.Servicios.First().SubTotal);
    }

    [TestMethod]
    public void AgregarServicio_SinHabitaciones_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();

        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.AgregarServicio(5, 1, policy.Object));
    }

    [TestMethod]
    public void AgregarServicio_ServicioDuplicado_LanzaConflictException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.AgregarServicio(5, 1, policy.Object);

        Assert.ThrowsExactly<ConflictException>(
            () => reserva.AgregarServicio(5, 1, policy.Object));
    }

    // -- QuitarServicio --------------------------------------------------------

    [TestMethod]
    public void QuitarServicio_ServicioExistente_LoElimina()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.AgregarServicio(5, 1, policy.Object);

        reserva.QuitarServicio(5);

        Assert.IsEmpty(reserva.Servicios);
    }

    [TestMethod]
    public void QuitarServicio_ServicioNoExistente_LanzaNotFoundException()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);

        Assert.ThrowsExactly<NotFoundException>(
            () => reserva.QuitarServicio(99));
    }

    // -- Confirmar -------------------------------------------------------------

    [TestMethod]
    public void Confirmar_ReservaPendienteConHabitacion_CambiaEstadoAConfirmada()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);

        reserva.Confirmar();

        Assert.AreEqual(EstadoReserva.Confirmada, reserva.EstadoReserva);
    }

    [TestMethod]
    public void Confirmar_SinHabitaciones_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);

        var ex = Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.Confirmar());
        Assert.Contains("habitacion", ex.Message.ToLower());
    }

    [TestMethod]
    public void Confirmar_ReservaYaConfirmada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();

        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.Confirmar());
    }

    // -- Cancelar --------------------------------------------------------------

    [TestMethod]
    public void Cancelar_ReservaPendiente_CambiaEstadoACancelada()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);

        reserva.Cancelar();

        Assert.AreEqual(EstadoReserva.Cancelada, reserva.EstadoReserva);
    }

    [TestMethod]
    public void Cancelar_ReservaFinalizada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba();
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.Confirmar();
        reserva.Finalizar();

        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.Cancelar());
    }

    [TestMethod]
    public void Cancelar_ReservaYaCancelada_LanzaBusinessRuleViolation()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        reserva.Cancelar();

        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => reserva.Cancelar());
    }

    // -- CostoTotal ------------------------------------------------------------

    [TestMethod]
    public void CostoTotal_DosHabitacionesUnServicio_CalculaCorrectamente()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        var policy = PolicyQueAprueba(tarifa: 1000m, precioServicio: 500m);
        reserva.AgregarHabitacion(10, policy.Object);
        reserva.AgregarHabitacion(11, policy.Object);
        reserva.AgregarServicio(5, 2, policy.Object); // 500 * 2 = 1000

        Assert.AreEqual(3000m, reserva.CostoTotal);
    }

    [TestMethod]
    public void CostoTotal_SinHabitacionesNiServicios_EsCero()
    {
        var reserva = new Reserva(1, Manana, PasadoManana);
        Assert.AreEqual(0m, reserva.CostoTotal);
    }
}

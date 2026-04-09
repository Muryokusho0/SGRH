using Xunit;
using SGRH.Domain.Entities.Clientes;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Entities;

public sealed class ClienteTests
{
    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DatosValidos_CreaClienteCorrectamente()
    {
        var cliente = new Cliente(
            "402-1234567-8", "Juan", "Pérez",
            "juan@email.com", "809-555-0000");

        Assert.Equal("402-1234567-8", cliente.NationalId);
        Assert.Equal("Juan", cliente.NombreCliente);
        Assert.Equal("Pérez", cliente.ApellidoCliente);
        Assert.Equal("juan@email.com", cliente.Email);
        Assert.Equal("809-555-0000", cliente.Telefono);
    }

    [Theory]
    [InlineData(null, "Juan", "Pérez", "j@e.com", "809")]
    [InlineData("402", null, "Pérez", "j@e.com", "809")]
    [InlineData("402", "Juan", null, "j@e.com", "809")]
    [InlineData("402", "Juan", "Pérez", null, "809")]
    [InlineData("402", "Juan", "Pérez", "j@e.com", null)]
    public void Constructor_CampoNulo_LanzaValidationException(
        string? nationalId, string? nombre, string? apellido,
        string? email, string? telefono)
    {
        Assert.Throws<ValidationException>(
            () => new Cliente(nationalId!, nombre!, apellido!, email!, telefono!));
    }

    [Fact]
    public void Constructor_NationalIdExcede20Chars_LanzaValidationException()
    {
        var idLargo = new string('1', 21);
        Assert.Throws<ValidationException>(
            () => new Cliente(idLargo, "Juan", "Pérez", "j@e.com", "809"));
    }

    [Fact]
    public void Constructor_NombreExcede100Chars_LanzaValidationException()
    {
        var nombreLargo = new string('a', 101);
        Assert.Throws<ValidationException>(
            () => new Cliente("402", nombreLargo, "Pérez", "j@e.com", "809"));
    }

    [Fact]
    public void Constructor_TelefonoExcede20Chars_LanzaValidationException()
    {
        var telLargo = new string('0', 21);
        Assert.Throws<ValidationException>(
            () => new Cliente("402", "Juan", "Pérez", "j@e.com", telLargo));
    }

    // ── ActualizarDatos ───────────────────────────────────────────────────────

    [Fact]
    public void ActualizarDatos_DatosValidos_ActualizaCorrectamente()
    {
        var cliente = new Cliente(
            "402", "Juan", "Pérez", "juan@e.com", "809");

        cliente.ActualizarDatos("María", "García", "maria@e.com", "829");

        Assert.Equal("María", cliente.NombreCliente);
        Assert.Equal("García", cliente.ApellidoCliente);
        Assert.Equal("maria@e.com", cliente.Email);
        Assert.Equal("829", cliente.Telefono);
        // NationalId NO cambia
        Assert.Equal("402", cliente.NationalId);
    }

    [Fact]
    public void ActualizarDatos_NombreVacio_LanzaValidationException()
    {
        var cliente = new Cliente("402", "Juan", "Pérez", "j@e.com", "809");
        Assert.Throws<ValidationException>(
            () => cliente.ActualizarDatos("", "García", "m@e.com", "829"));
    }
}
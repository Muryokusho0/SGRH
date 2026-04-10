using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGRH.Domain.Entities.Clientes;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Entities;

[TestClass]
public sealed class ClienteTests
{
    [TestMethod]
    public void Constructor_DatosValidos_CreaClienteCorrectamente()
    {
        var cliente = new Cliente(
            "402-1234567-8", "Juan", "Pérez",
            "juan@email.com", "809-555-0000");

        Assert.AreEqual("402-1234567-8", cliente.NationalId);
        Assert.AreEqual("Juan", cliente.NombreCliente);
        Assert.AreEqual("Pérez", cliente.ApellidoCliente);
        Assert.AreEqual("juan@email.com", cliente.Email);
        Assert.AreEqual("809-555-0000", cliente.Telefono);
    }

    [TestMethod]
    [DataRow(null, "Juan", "Pérez", "j@e.com", "809")]
    [DataRow("402", null, "Pérez", "j@e.com", "809")]
    [DataRow("402", "Juan", null, "j@e.com", "809")]
    [DataRow("402", "Juan", "Pérez", null, "809")]
    [DataRow("402", "Juan", "Pérez", "j@e.com", null)]
    public void Constructor_CampoNulo_LanzaValidationException(
        string? nationalId, string? nombre, string? apellido,
        string? email, string? telefono)
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Cliente(nationalId!, nombre!, apellido!, email!, telefono!));
    }

    [TestMethod]
    public void Constructor_NationalIdExcede20Chars_LanzaValidationException()
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Cliente(new string('1', 21), "Juan", "Pérez", "j@e.com", "809"));
    }

    [TestMethod]
    public void Constructor_NombreExcede100Chars_LanzaValidationException()
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Cliente("402", new string('a', 101), "Pérez", "j@e.com", "809"));
    }

    [TestMethod]
    public void Constructor_TelefonoExcede20Chars_LanzaValidationException()
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Cliente("402", "Juan", "Pérez", "j@e.com", new string('0', 21)));
    }

    [TestMethod]
    public void ActualizarDatos_DatosValidos_ActualizaCorrectamente()
    {
        var cliente = new Cliente("402", "Juan", "Pérez", "juan@e.com", "809");

        cliente.ActualizarDatos("María", "García", "maria@e.com", "829");

        Assert.AreEqual("María", cliente.NombreCliente);
        Assert.AreEqual("García", cliente.ApellidoCliente);
        Assert.AreEqual("maria@e.com", cliente.Email);
        Assert.AreEqual("829", cliente.Telefono);
        Assert.AreEqual("402", cliente.NationalId); // no cambia
    }

    [TestMethod]
    public void ActualizarDatos_NombreVacio_LanzaValidationException()
    {
        var cliente = new Cliente("402", "Juan", "Pérez", "j@e.com", "809");

        Assert.ThrowsExactly<ValidationException>(
            () => cliente.ActualizarDatos("", "García", "m@e.com", "829"));
    }
}

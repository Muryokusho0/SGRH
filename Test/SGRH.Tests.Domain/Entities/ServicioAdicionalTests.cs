using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Entities;

[TestClass]
public sealed class ServicioAdicionalTests
{
    [TestMethod]
    public void Constructor_DatosValidos_CreaServicioSinTemporadas()
    {
        var svc = new ServicioAdicional("Desayuno Buffet", "Alimentación");

        Assert.AreEqual("Desayuno Buffet", svc.NombreServicio);
        Assert.AreEqual("Alimentación", svc.TipoServicio);
        Assert.IsFalse(svc.AplicaTodasTemporadas);
        Assert.IsEmpty(svc.TemporadaIds);
    }

    [TestMethod]
    [DataRow("", "Alimentación")]
    [DataRow("Desayuno", "")]
    public void Constructor_CampoVacio_LanzaValidationException(
        string nombre, string tipo)
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new ServicioAdicional(nombre, tipo));
    }

    [TestMethod]
    public void EstaDisponibleEn_AplicaTodasTemporadas_SiempreRetornaTrue()
    {
        var svc = new ServicioAdicional("Spa", "Bienestar");
        svc.HabilitarParaTodasTemporadas();

        Assert.IsTrue(svc.EstaDisponibleEn(null));
        Assert.IsTrue(svc.EstaDisponibleEn(1));
        Assert.IsTrue(svc.EstaDisponibleEn(99));
    }

    [TestMethod]
    public void EstaDisponibleEn_ServicioEspecifico_SinTemporadaActiva_RetornaFalse()
    {
        var svc = new ServicioAdicional("Tour Navidad", "Entretenimiento");
        svc.HabilitarEnTemporada(5);

        Assert.IsFalse(svc.EstaDisponibleEn(null));
    }

    [TestMethod]
    public void EstaDisponibleEn_ServicioEspecifico_TemporadaCorrecta_RetornaTrue()
    {
        var svc = new ServicioAdicional("Tour Navidad", "Entretenimiento");
        svc.HabilitarEnTemporada(5);

        Assert.IsTrue(svc.EstaDisponibleEn(5));
    }

    [TestMethod]
    public void EstaDisponibleEn_ServicioEspecifico_TemporadaIncorrecta_RetornaFalse()
    {
        var svc = new ServicioAdicional("Tour Navidad", "Entretenimiento");
        svc.HabilitarEnTemporada(5);

        Assert.IsFalse(svc.EstaDisponibleEn(9));
    }

    [TestMethod]
    public void HabilitarEnTemporada_NuevaTemporada_AgregaCorrectamente()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        svc.HabilitarEnTemporada(3);
        Assert.Contains(3, svc.TemporadaIds);
    }

    [TestMethod]
    public void HabilitarEnTemporada_TemporadaDuplicada_LanzaConflictException()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        svc.HabilitarEnTemporada(3);
        Assert.ThrowsExactly<ConflictException>(
            () => svc.HabilitarEnTemporada(3));
    }

    [TestMethod]
    public void DeshabilitarEnTemporada_TemporadaExistente_LaElimina()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        svc.HabilitarEnTemporada(3);
        svc.DeshabilitarEnTemporada(3);
        Assert.DoesNotContain(3, svc.TemporadaIds);
    }

    [TestMethod]
    public void DeshabilitarEnTemporada_TemporadaNoExistente_LanzaNotFoundException()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        Assert.ThrowsExactly<NotFoundException>(
            () => svc.DeshabilitarEnTemporada(99));
    }
}

using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Exceptions;
using Xunit;

namespace SGRH.Tests.Domain.Entities;

public sealed class ServicioAdicionalTests
{
    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DatosValidos_CreaServicioSinTemporadas()
    {
        var svc = new ServicioAdicional("Desayuno Buffet", "Alimentación");

        Assert.Equal("Desayuno Buffet", svc.NombreServicio);
        Assert.Equal("Alimentación", svc.TipoServicio);
        Assert.False(svc.AplicaTodasTemporadas);
        Assert.Empty(svc.TemporadaIds);
    }

    [Theory]
    [InlineData("", "Alimentación")]
    [InlineData("Desayuno", "")]
    public void Constructor_CampoVacio_LanzaValidationException(
        string nombre, string tipo)
    {
        Assert.Throws<ValidationException>(
            () => new ServicioAdicional(nombre, tipo));
    }

    // ── EstaDisponibleEn ──────────────────────────────────────────────────────

    [Fact]
    public void EstaDisponibleEn_AplicaTodasTemporadas_SiempreRetornaTrue()
    {
        var svc = new ServicioAdicional("Spa", "Bienestar");
        svc.HabilitarParaTodasTemporadas();

        Assert.True(svc.EstaDisponibleEn(null));
        Assert.True(svc.EstaDisponibleEn(1));
        Assert.True(svc.EstaDisponibleEn(99));
    }

    [Fact]
    public void EstaDisponibleEn_ServicioEspecifico_SinTemporadaActiva_RetornaFalse()
    {
        // Regla corregida: servicio específico NO disponible sin temporada activa
        var svc = new ServicioAdicional("Tour Navidad", "Entretenimiento");
        svc.HabilitarEnTemporada(5);

        Assert.False(svc.EstaDisponibleEn(null));
    }

    [Fact]
    public void EstaDisponibleEn_ServicioEspecifico_TemporadaCorrecta_RetornaTrue()
    {
        var svc = new ServicioAdicional("Tour Navidad", "Entretenimiento");
        svc.HabilitarEnTemporada(5);

        Assert.True(svc.EstaDisponibleEn(5));
    }

    [Fact]
    public void EstaDisponibleEn_ServicioEspecifico_TemporadaIncorrecta_RetornaFalse()
    {
        var svc = new ServicioAdicional("Tour Navidad", "Entretenimiento");
        svc.HabilitarEnTemporada(5);

        Assert.False(svc.EstaDisponibleEn(9)); // temporada diferente
    }

    // ── HabilitarEnTemporada ──────────────────────────────────────────────────

    [Fact]
    public void HabilitarEnTemporada_NuevaTem_AgregaCorrectamente()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        svc.HabilitarEnTemporada(3);
        Assert.Contains(3, svc.TemporadaIds);
    }

    [Fact]
    public void HabilitarEnTemporada_TemporadaDuplicada_LanzaConflictException()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        svc.HabilitarEnTemporada(3);
        Assert.Throws<ConflictException>(() => svc.HabilitarEnTemporada(3));
    }

    [Fact]
    public void DeshabilitarEnTemporada_TemporadaExistente_LaElimina()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        svc.HabilitarEnTemporada(3);
        svc.DeshabilitarEnTemporada(3);
        Assert.DoesNotContain(3, svc.TemporadaIds);
    }

    [Fact]
    public void DeshabilitarEnTemporada_TemporadaNoExistente_LanzaNotFoundException()
    {
        var svc = new ServicioAdicional("Tour", "Entretenimiento");
        Assert.Throws<NotFoundException>(() => svc.DeshabilitarEnTemporada(99));
    }
}
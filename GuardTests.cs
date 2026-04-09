using Xunit;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Common;

/// <summary>
/// Pruebas unitarias de Guard — los guard clauses son la primera línea
/// de defensa de las invariantes del dominio en todas las entidades.
/// </summary>
public sealed class GuardTests
{
    // ── AgainstNullOrWhiteSpace ───────────────────────────────────────────────

    [Fact]
    public void AgainstNullOrWhiteSpace_ValorValido_NoLanzaExcepcion()
    {
        // Arrange & Act & Assert — no debe lanzar
        var ex = Record.Exception(
            () => Guard.AgainstNullOrWhiteSpace("valor", "campo", 50));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_ValorVacioONulo_LanzaValidationException(
        string? valor)
    {
        var ex = Assert.Throws<ValidationException>(
            () => Guard.AgainstNullOrWhiteSpace(valor, "campo", 50));
        Assert.Contains("campo", ex.Message);
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_ValorExcedeMaxLength_LanzaValidationException()
    {
        var valor = new string('a', 51); // 51 > maxLength 50
        var ex = Assert.Throws<ValidationException>(
            () => Guard.AgainstNullOrWhiteSpace(valor, "campo", 50));
        Assert.Contains("50", ex.Message);
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_ValorExactoMaxLength_NoLanzaExcepcion()
    {
        var valor = new string('a', 50); // exactamente el límite
        var ex = Record.Exception(
            () => Guard.AgainstNullOrWhiteSpace(valor, "campo", 50));
        Assert.Null(ex);
    }

    // ── AgainstOutOfRange (int) ───────────────────────────────────────────────

    [Fact]
    public void AgainstOutOfRange_ValorPositivo_NoLanzaExcepcion()
    {
        var ex = Record.Exception(() => Guard.AgainstOutOfRange(1, "campo", 0));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AgainstOutOfRange_ValorMenorOIgualAMinimo_LanzaValidationException(
        int valor)
    {
        var ex = Assert.Throws<ValidationException>(
            () => Guard.AgainstOutOfRange(valor, "campo", 0));
        Assert.Contains("campo", ex.Message);
    }

    // ── AgainstOutOfRange (decimal) ───────────────────────────────────────────

    [Fact]
    public void AgainstOutOfRange_DecimalPositivo_NoLanzaExcepcion()
    {
        var ex = Record.Exception(
            () => Guard.AgainstOutOfRange(0.01m, "precio", 0m));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AgainstOutOfRange_DecimalMenorOIgualACero_LanzaValidationException(
        double valor)
    {
        var ex = Assert.Throws<ValidationException>(
            () => Guard.AgainstOutOfRange((decimal)valor, "precio", 0m));
        Assert.Contains("precio", ex.Message);
    }

    // ── AgainstInvalidDateRange ───────────────────────────────────────────────

    [Fact]
    public void AgainstInvalidDateRange_RangoValido_NoLanzaExcepcion()
    {
        var entrada = DateTime.Today.AddDays(1);
        var salida = DateTime.Today.AddDays(3);
        var ex = Record.Exception(
            () => Guard.AgainstInvalidDateRange(
                entrada, salida, "entrada", "salida"));
        Assert.Null(ex);
    }

    [Fact]
    public void AgainstInvalidDateRange_EntradaIgualASalida_LanzaValidationException()
    {
        var fecha = DateTime.Today.AddDays(1);
        var ex = Assert.Throws<ValidationException>(
            () => Guard.AgainstInvalidDateRange(fecha, fecha, "entrada", "salida"));
        Assert.Contains("entrada", ex.Message);
    }

    [Fact]
    public void AgainstInvalidDateRange_EntradaPosteriorASalida_LanzaValidationException()
    {
        var entrada = DateTime.Today.AddDays(5);
        var salida = DateTime.Today.AddDays(2);
        Assert.Throws<ValidationException>(
            () => Guard.AgainstInvalidDateRange(
                entrada, salida, "entrada", "salida"));
    }
}
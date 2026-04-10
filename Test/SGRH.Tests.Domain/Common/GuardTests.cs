using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Common;

[TestClass]
public sealed class GuardTests
{
    // -- AgainstNullOrWhiteSpace -----------------------------------------------

    [TestMethod]
    public void AgainstNullOrWhiteSpace_ValorValido_NoLanzaExcepcion()
    {
        var ex = RecordException(
            () => Guard.AgainstNullOrWhiteSpace("valor", "campo", 50));
        Assert.IsNull(ex);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void AgainstNullOrWhiteSpace_ValorVacioONulo_LanzaValidationException(
        string? valor)
    {
        var ex = Assert.ThrowsExactly<ValidationException>(
            () => Guard.AgainstNullOrWhiteSpace(valor, "campo", 50));
        StringAssert.Contains(ex.Message, "campo");
    }

    [TestMethod]
    public void AgainstNullOrWhiteSpace_ValorExcedeMaxLength_LanzaValidationException()
    {
        var valor = new string('a', 51);
        var ex = Assert.ThrowsExactly<ValidationException>(
            () => Guard.AgainstNullOrWhiteSpace(valor, "campo", 50));
        StringAssert.Contains(ex.Message, "50");
    }

    [TestMethod]
    public void AgainstNullOrWhiteSpace_ValorExactoMaxLength_NoLanzaExcepcion()
    {
        var valor = new string('a', 50);
        var ex = RecordException(
            () => Guard.AgainstNullOrWhiteSpace(valor, "campo", 50));
        Assert.IsNull(ex);
    }

    // -- AgainstOutOfRange (int) -----------------------------------------------

    [TestMethod]
    public void AgainstOutOfRange_ValorPositivo_NoLanzaExcepcion()
    {
        var ex = RecordException(() => Guard.AgainstOutOfRange(1, "campo", 0));
        Assert.IsNull(ex);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    public void AgainstOutOfRange_ValorMenorOIgualAMinimo_LanzaValidationException(
        int valor)
    {
        Assert.ThrowsExactly<ValidationException>(
            () => Guard.AgainstOutOfRange(valor, "campo", 0));
    }

    // -- AgainstOutOfRange (decimal) -------------------------------------------

    [TestMethod]
    public void AgainstOutOfRange_DecimalPositivo_NoLanzaExcepcion()
    {
        var ex = RecordException(
            () => Guard.AgainstOutOfRange(0.01m, "precio", 0m));
        Assert.IsNull(ex);
    }

    [TestMethod]
    [DataRow(0.0)]
    [DataRow(-1.0)]
    public void AgainstOutOfRange_DecimalMenorOIgualACero_LanzaValidationException(
        double valor)
    {
        Assert.ThrowsExactly<ValidationException>(
            () => Guard.AgainstOutOfRange((decimal)valor, "precio", 0m));
    }

    // -- AgainstInvalidDateRange -----------------------------------------------

    [TestMethod]
    public void AgainstInvalidDateRange_RangoValido_NoLanzaExcepcion()
    {
        var entrada = DateTime.Today.AddDays(1);
        var salida = DateTime.Today.AddDays(3);
        var ex = RecordException(
            () => Guard.AgainstInvalidDateRange(
                entrada, salida, "entrada", "salida"));
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void AgainstInvalidDateRange_EntradaIgualASalida_LanzaValidationException()
    {
        var fecha = DateTime.Today.AddDays(1);
        Assert.ThrowsExactly<ValidationException>(
            () => Guard.AgainstInvalidDateRange(
                fecha, fecha, "entrada", "salida"));
    }

    [TestMethod]
    public void AgainstInvalidDateRange_EntradaPosteriorASalida_LanzaValidationException()
    {
        var entrada = DateTime.Today.AddDays(5);
        var salida = DateTime.Today.AddDays(2);
        Assert.ThrowsExactly<ValidationException>(
            () => Guard.AgainstInvalidDateRange(
                entrada, salida, "entrada", "salida"));
    }

    // -- Helper ----------------------------------------------------------------

    private static Exception? RecordException(Action action)
    {
        try { action(); return null; }
        catch (Exception ex) { return ex; }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGRH.Application.UseCases.Reservas.AgregarHabitacion;

namespace SGRH.Tests.Application.Validators;

[TestClass]
public sealed class AgregarHabitacionValidatorTests
{
    private readonly AgregarHabitacionValidator _validator = new();

    private static AgregarHabitacionRequest Req(int reservaId, int habitacionId) =>
        new(reservaId, habitacionId,
            new(Guid.NewGuid(), "127.0.0.1", "Test/1.0"));

    [TestMethod]
    public async Task ValidarAsync_IdsValidos_RetornaSuccess()
    {
        var r = await _validator.ValidateAsync(Req(1, 10), TestContext.CancellationToken);
        Assert.IsTrue(r.IsValid);
    }

    [TestMethod]
    [DataRow(0, 10)]
    [DataRow(-1, 10)]
    public async Task ValidarAsync_ReservaIdInvalido_RetornaError(
        int reservaId, int habitacionId)
    {
        var r = await _validator.ValidateAsync(Req(reservaId, habitacionId), TestContext.CancellationToken);
        Assert.IsFalse(r.IsValid);
        Assert.Contains(e => e.Contains("ReservaId"), r.Errors);
    }

    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(1, -5)]
    public async Task ValidarAsync_HabitacionIdInvalido_RetornaError(
        int reservaId, int habitacionId)
    {
        var r = await _validator.ValidateAsync(Req(reservaId, habitacionId), TestContext.CancellationToken);
        Assert.IsFalse(r.IsValid);
        Assert.Contains(e => e.Contains("HabitacionId"), r.Errors);
    }

    public TestContext TestContext { get; set; }
}

using SGRH.Application.UseCases.Reservas.AgregarHabitacion;
using System;
using Xunit;
using System.Threading.Tasks;

namespace SGRH.Tests.Application.Validators;

public sealed class AgregarHabitacionValidatorTests
{
    private readonly AgregarHabitacionValidator _validator = new();

    private static AgregarHabitacionRequest Req(int reservaId, int habitacionId)
        => new(reservaId, habitacionId,
               new(Guid.NewGuid(), "127.0.0.1", "TestAgent/1.0"));

    [Fact]
    public async Task ValidarAsync_IdsValidos_RetornaSuccess()
    {
        var r = await _validator.ValidateAsync(Req(1, 10));
        Assert.True(r.IsValid);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public async Task ValidarAsync_ReservaIdInvalido_RetornaError(
        int reservaId, int habitacionId)
    {
        var r = await _validator.ValidateAsync(Req(reservaId, habitacionId));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.Contains("ReservaId"));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public async Task ValidarAsync_HabitacionIdInvalido_RetornaError(
        int reservaId, int habitacionId)
    {
        var r = await _validator.ValidateAsync(Req(reservaId, habitacionId));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.Contains("HabitacionId"));
    }
}
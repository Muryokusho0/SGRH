using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;
using Xunit;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Entities;

public sealed class UsuarioTests
{
    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ClienteConClienteId_CreaUsuarioActivo()
    {
        var usuario = new Usuario(
            clienteId: 1, "cliente@email.com",
            "hash_seguro", RolUsuario.CLIENTE);

        Assert.Equal(1, usuario.ClienteId);
        Assert.Equal("cliente@email.com", usuario.Username);
        Assert.Equal(RolUsuario.CLIENTE, usuario.Rol);
        Assert.True(usuario.Activo);
    }

    [Fact]
    public void Constructor_AdminSinClienteId_CreaUsuarioCorrectamente()
    {
        var usuario = new Usuario(
            clienteId: null, "admin@sgrh.com",
            "hash_seguro", RolUsuario.ADMIN);

        Assert.Null(usuario.ClienteId);
        Assert.Equal(RolUsuario.ADMIN, usuario.Rol);
    }

    [Fact]
    public void Constructor_ClienteSinClienteId_LanzaBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new Usuario(null, "u@e.com", "hash", RolUsuario.CLIENTE));
        Assert.Contains("ClienteId", ex.Message);
    }

    [Fact]
    public void Constructor_AdminConClienteId_LanzaBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new Usuario(5, "admin@e.com", "hash", RolUsuario.ADMIN));
        Assert.Contains("ClienteId", ex.Message);
    }

    [Fact]
    public void Constructor_RecepcionistaConClienteId_LanzaBusinessRuleViolation()
    {
        Assert.Throws<BusinessRuleViolationException>(
            () => new Usuario(3, "recep@e.com", "hash", RolUsuario.RECEPCIONISTA));
    }

    [Fact]
    public void Constructor_UsernameVacio_LanzaValidationException()
    {
        Assert.Throws<ValidationException>(
            () => new Usuario(1, "", "hash", RolUsuario.CLIENTE));
    }

    // ── Desactivar / Activar ──────────────────────────────────────────────────

    [Fact]
    public void Desactivar_UsuarioActivo_LoPoneInactivo()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        usuario.Desactivar();
        Assert.False(usuario.Activo);
    }

    [Fact]
    public void Desactivar_UsuarioYaInactivo_LanzaBusinessRuleViolation()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        usuario.Desactivar();
        Assert.Throws<BusinessRuleViolationException>(() => usuario.Desactivar());
    }

    [Fact]
    public void Activar_UsuarioInactivo_LoPoneActivo()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        usuario.Desactivar();
        usuario.Activar();
        Assert.True(usuario.Activo);
    }

    [Fact]
    public void Activar_UsuarioYaActivo_LanzaBusinessRuleViolation()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        Assert.Throws<BusinessRuleViolationException>(() => usuario.Activar());
    }

    // ── CambiarPassword ───────────────────────────────────────────────────────

    [Fact]
    public void CambiarPassword_NuevoHashValido_ActualizaPasswordHash()
    {
        var usuario = new Usuario(1, "u@e.com", "hash_viejo", RolUsuario.CLIENTE);
        usuario.CambiarPassword("hash_nuevo_seguro");
        Assert.Equal("hash_nuevo_seguro", usuario.PasswordHash);
    }

    [Fact]
    public void CambiarPassword_HashVacio_LanzaValidationException()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        Assert.Throws<ValidationException>(() => usuario.CambiarPassword(""));
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Tests.Domain.Entities;

[TestClass]
public sealed class UsuarioTests
{
    [TestMethod]
    public void Constructor_ClienteConClienteId_CreaUsuarioActivo()
    {
        var usuario = new Usuario(1, "cliente@email.com", "hash", RolUsuario.CLIENTE);

        Assert.AreEqual(1, usuario.ClienteId);
        Assert.AreEqual("cliente@email.com", usuario.Username);
        Assert.AreEqual(RolUsuario.CLIENTE, usuario.Rol);
        Assert.IsTrue(usuario.Activo);
    }

    [TestMethod]
    public void Constructor_AdminSinClienteId_CreaUsuarioCorrectamente()
    {
        var usuario = new Usuario(null, "admin@sgrh.com", "hash", RolUsuario.ADMIN);

        Assert.IsNull(usuario.ClienteId);
        Assert.AreEqual(RolUsuario.ADMIN, usuario.Rol);
    }

    [TestMethod]
    public void Constructor_ClienteSinClienteId_LanzaBusinessRuleViolation()
    {
        var ex = Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => new Usuario(null, "u@e.com", "hash", RolUsuario.CLIENTE));
        Assert.Contains("ClienteId", ex.Message);
    }

    [TestMethod]
    public void Constructor_AdminConClienteId_LanzaBusinessRuleViolation()
    {
        var ex = Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => new Usuario(5, "admin@e.com", "hash", RolUsuario.ADMIN));
        Assert.Contains("ClienteId", ex.Message);
    }

    [TestMethod]
    public void Constructor_RecepcionistaConClienteId_LanzaBusinessRuleViolation()
    {
        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => new Usuario(3, "recep@e.com", "hash", RolUsuario.RECEPCIONISTA));
    }

    [TestMethod]
    public void Constructor_UsernameVacio_LanzaValidationException()
    {
        Assert.ThrowsExactly<ValidationException>(
            () => new Usuario(1, "", "hash", RolUsuario.CLIENTE));
    }

    [TestMethod]
    public void Desactivar_UsuarioActivo_LoPoneInactivo()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        usuario.Desactivar();
        Assert.IsFalse(usuario.Activo);
    }

    [TestMethod]
    public void Desactivar_UsuarioYaInactivo_LanzaBusinessRuleViolation()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        usuario.Desactivar();
        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => usuario.Desactivar());
    }

    [TestMethod]
    public void Activar_UsuarioInactivo_LoPoneActivo()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        usuario.Desactivar();
        usuario.Activar();
        Assert.IsTrue(usuario.Activo);
    }

    [TestMethod]
    public void Activar_UsuarioYaActivo_LanzaBusinessRuleViolation()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        Assert.ThrowsExactly<BusinessRuleViolationException>(
            () => usuario.Activar());
    }

    [TestMethod]
    public void CambiarPassword_HashValido_Actualiza()
    {
        var usuario = new Usuario(1, "u@e.com", "hash_viejo", RolUsuario.CLIENTE);
        usuario.CambiarPassword("hash_nuevo");
        Assert.AreEqual("hash_nuevo", usuario.PasswordHash);
    }

    [TestMethod]
    public void CambiarPassword_HashVacio_LanzaValidationException()
    {
        var usuario = new Usuario(1, "u@e.com", "hash", RolUsuario.CLIENTE);
        Assert.ThrowsExactly<ValidationException>(
            () => usuario.CambiarPassword(""));
    }
}

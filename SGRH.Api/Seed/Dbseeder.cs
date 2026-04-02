using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Auth;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;
using SGRH.Persistence.Context;

namespace SGRH.Api.Seed;

/// <summary>
/// Inicializa el usuario administrador por defecto cuando la base de datos está vacía.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Ejecuta el seed inicial de datos (admin) si no existe un usuario ADMIN activo.
    /// </summary>
    /// <param name="app">Aplicación en ejecución.</param>
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<SGRHDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Aplica migraciones pendientes automáticamente en desarrollo
        if (app.Environment.IsDevelopment())
            await db.Database.MigrateAsync();

        // Si ya existe al menos un ADMIN activo, no hacer nada
        var adminExiste = await db.Usuarios
            .AnyAsync(u => u.Rol == RolUsuario.ADMIN && u.Activo);

        if (adminExiste)
            return;

        // Leer credenciales del admin inicial desde configuración
        var email = config["Seed:AdminEmail"]
            ?? throw new InvalidOperationException(
                "Seed:AdminEmail no está configurado. " +
                "Agrégalo en appsettings.Development.json o como variable de entorno.");

        var password = config["Seed:AdminPassword"]
            ?? throw new InvalidOperationException(
                "Seed:AdminPassword no está configurado. " +
                "Agrégalo en appsettings.Development.json o como variable de entorno.");

        // Validar longitud mínima de la contraseña del seed
        if (password.Length < 8)
            throw new InvalidOperationException(
                "Seed:AdminPassword debe tener al menos 8 caracteres.");

        var admin = new Usuario(
            clienteId: null,
            username: email,
            passwordHash: hasher.Hash(password),
            rol: RolUsuario.ADMIN);

        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Admin inicial creado: {Email}. Cambia la contraseña después del primer login.",
            email);
    }
}
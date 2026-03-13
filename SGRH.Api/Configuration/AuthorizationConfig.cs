using Microsoft.Extensions.DependencyInjection;

namespace SGRH.Api.Configuration;

public static class AuthorizationConfig
{
    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(opt =>
        {
            // Solo administradores
            opt.AddPolicy("SoloAdmin", p =>
                p.RequireRole("ADMIN"));

            // Solo clientes registrados
            opt.AddPolicy("SoloCliente", p =>
                p.RequireRole("CLIENTE"));

            // Administrador o Recepcionista (gestión operativa)
            opt.AddPolicy("AdminORecepcionista", p =>
                p.RequireRole("ADMIN", "RECEPCIONISTA"));

            // Administrador, Recepcionista o Cliente autenticado
            opt.AddPolicy("Autenticado", p =>
                p.RequireAuthenticatedUser());
        });

        return services;
    }
}
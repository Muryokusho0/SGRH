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

            // Administrador o Recepcionista (gestión operativa)
            opt.AddPolicy("AdminORecepcionista", p =>
                p.RequireRole("ADMIN", "RECEPCIONISTA"));

            // Cualquier usuario autenticado
            opt.AddPolicy("Autenticado", p =>
                p.RequireAuthenticatedUser());
        });

        return services;
    }
}
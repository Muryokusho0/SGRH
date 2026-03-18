using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SGRH.Auth.Hashing;
using SGRH.Auth.Jwt;
using SGRH.Domain.Abstractions.Auth;

namespace SGRH.Auth.DependencyInjection;

public static class AuthDependencyInjection
{
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddJwtGenerator(config)
            .AddJwtBearer(config)
            .AddPasswordHasher();

        return services;
    }

    // ── 1. Generador de tokens ────────────────────────────────────────────
    private static IServiceCollection AddJwtGenerator(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(opt =>
            config.GetSection(JwtOptions.Section).Bind(opt));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    // ── 2. Middleware JWT Bearer ──────────────────────────────────────────
    private static IServiceCollection AddJwtBearer(
        this IServiceCollection services, IConfiguration config)
    {
        var key = config["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key no está configurado. Agrégalo en appsettings o en las variables de entorno.");

        var issuer = config["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer no está configurado.");

        var audience = config["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience no está configurado.");

        // HS256 requiere mínimo 256 bits = 32 bytes UTF-8.
        // Fallamos en startup con mensaje claro en lugar de explotar en el primer request.
        var keyBytes = Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException(
                $"Jwt:Key es demasiado corta ({keyBytes.Length * 8} bits). " +
                $"HS256 requiere mínimo 256 bits (32 caracteres ASCII). " +
                $"Genera una clave segura de al menos 32 caracteres y actualiza la configuración.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero,

                    // Esencial: le dice al handler exactamente qué claim type
                    // usar para rol y nombre al validar [Authorize(Policy = "...")]
                    // y RequireRole(). Sin esto, el JWT bearer puede no encontrar
                    // el claim correcto y devolver 401 aunque el token sea válido.
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name,
                };
            });

        return services;
    }

    // ── 3. Password hasher ────────────────────────────────────────────────
    private static IServiceCollection AddPasswordHasher(
        this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        return services;
    }
}
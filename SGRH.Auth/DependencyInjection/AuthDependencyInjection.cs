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

    // ── 2. Middleware JWT Bearer (valida tokens en cada request) ──────────

    private static IServiceCollection AddJwtBearer(
        this IServiceCollection services, IConfiguration config)
    {
        var key = config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key no configurado.");
        var issuer = config["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");
        var audience = config["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience no configurado.");

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
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero  // Sin tolerancia de tiempo
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
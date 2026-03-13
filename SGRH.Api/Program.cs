using Microsoft.EntityFrameworkCore;
using SGRH.Api.Configuration;
using SGRH.Application.DependencyInjection;
using SGRH.Auth.DependencyInjection;
using SGRH.Infrastructure.DependencyInjection;
using SGRH.Persistence.Context;

namespace SGRH.Api;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Servicios ─────────────────────────────────────────────────
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
        {
            // Permite enviar el token JWT desde Swagger
            opt.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Ingresa el token JWT. Ejemplo: Bearer {token}"
            });
            opt.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Infrastructure: DbContext, repositorios, S3, SES, domain services
        builder.Services.AddInfrastructure(builder.Configuration);

        // Auth: JWT generator, JWT bearer middleware, BCrypt
        builder.Services.AddAuth(builder.Configuration);

        // Policies por rol
        builder.Services.AddAuthorizationPolicies();

        //Aplication: MediatR, AutoMapper, validadores, servicios de aplicación
        builder.Services.AddApplication();

        // ── Pipeline ──────────────────────────────────────────────────
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();   // ← debe ir antes de UseAuthorization
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
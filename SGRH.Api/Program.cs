using SGRH.Api.Configuration;
using SGRH.Api.Middleware;
using SGRH.Api.Seed;
using SGRH.Application.DependencyInjection;
using SGRH.Auth.DependencyInjection;
using SGRH.Infrastructure.DependencyInjection;
using SGRH.Api.Converters;

namespace SGRH.Api;

/// <summary>
/// Punto de entrada de la API. Configura servicios, middleware y arranca el host.
/// </summary>
public class Program
{
    /// <summary>
    /// Inicializa la aplicación web, registra dependencias y ejecuta el pipeline.
    /// </summary>
    /// <param name="args">Argumentos de línea de comandos.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Servicios ─────────────────────────────────────────────────────
        builder.Services.AddControllers()
            .AddJsonOptions(opt =>
             {
                 opt.JsonSerializerOptions.Converters.Add(
                     new DateTimeLocalConverter());
             });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
        {
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

        // Policies por rol: SoloAdmin, SoloCliente, AdminORecepcionista, Autenticado
        builder.Services.AddAuthorizationPolicies();

        // Application: validadores, UseCases
        builder.Services.AddApplication();

        // ── Pipeline ──────────────────────────────────────────────────────
        var app = builder.Build();

        // ① Excepciones — siempre primero para capturar todo el pipeline
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // ② Seed — crea el primer admin si no existe ninguno
        await DbSeeder.SeedAsync(app);

        app.Run();
    }
}
using Microsoft.EntityFrameworkCore;
using SGRH.Persistence.Context;
using SGRH.Infrastructure.DependencyInjection;

namespace SGRH.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Servicios ─────────────────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Registra toda la infraestructura: DbContext, repositorios,
            // S3, SES, UnitOfWork, domain services
            builder.Services.AddInfrastructure(builder.Configuration);

            // ── Pipeline ──────────────────────────────────────────────────
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
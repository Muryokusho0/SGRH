using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGRH.Persistence.Context;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Persistence.Repositories;
using SGRH.Persistence.UnitOfWork;
using SGRH.Domain.Abstractions.Storage;
using SGRH.Domain.Abstractions.Email;
using SGRH.Domain.Services.Time;
// using SGRH.Infrastructure.Email;
// using SGRH.Infrastructure.Storage;
// using SGRH.Infrastructure.Time;

namespace SGRH.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── 1. DbContext ─────────────────────────────────────────
        services.AddDbContext<SGRHDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("Default")));

        // ── 2. UnitOfWork ────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── 3. Repositorios EF ───────────────────────────────────
        services.AddScoped<IAuditoriaRepository, AuditoriaRepositoryEF>();
        services.AddScoped<ICategoriaHabitacionRepository, CategoriaHabitacionRepositoryEF>();
        services.AddScoped<IClienteRepository, ClienteRepositoryEF>();
        services.AddScoped<IHabitacionHistorialRepository, HabitacionHistorialRepositoryEF>();
        services.AddScoped<IHabitacionRepository, HabitacionRepositoryEF>();
        services.AddScoped<IReservaRepository, ReservaRepositoryEF>();
        services.AddScoped<IServicioAdicionalRepository, ServicioAdicionalRepositoryEF>();
        services.AddScoped<IServicioCategoriaPrecioRepository, ServicioCategoriaPrecioRepositoryEF>();
        services.AddScoped<ITarifaTemporadaRepository, TarifaTemporadaRepositoryEF>();
        services.AddScoped<ITemporadaRepository, TemporadaRepositoryEF>();
        services.AddScoped<IUsuarioRepository, UsuarioRepositoryEF>();


        // Descomentar cuando las implementaciones estén listas:
        // services.AddScoped<ISystemClock, SystemClock>();
        // services.AddScoped<IEmailSender, SesEmailSender>();
        // services.AddScoped<IFileStorage, S3FileStorage>();
        // services.AddScoped<IAdminNotifier, AdminNotifier>();

        return services;
    }
}
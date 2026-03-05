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

namespace SGRH.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // 1) DbContext (Persistence)
        var cs = config.GetConnectionString("Default");
        services.AddDbContext<SGRHDbContext>(opt =>
            opt.UseSqlServer(cs));

        // 2) UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 3) Repositorios EF (Persistence)
        services.AddScoped<IAuditoriaRepository, AuditoriaRepositoryEF>();
        services.AddScoped<ICategoriaHabitacionRepository, CategoriaHabitacionRepositoryEF>();
        services.AddScoped<IClienteRepository, ClienteRepositoryEF>();
        services.AddScoped<IDetalleReservaRepository, DetalleReservaRepositoryEF>();
        services.AddScoped<IHabitacionHistorialRepository, HabitacionHistorialRepositoryEF>();
        services.AddScoped<IHabitacionRepository, HabitacionRepositoryEF>();
        services.AddScoped<IReservaRepository, ReservaRepositoryEF>();
        services.AddScoped<IReservaServicioAdicionalRepository, ReservaServicioAdicionalRepositoryEF>();
        services.AddScoped<IServicioCategoriaPrecioRepository, ServicioCategoriaPrecioRepositoryEF>();
        services.AddScoped<IServicioAdicionalRepository, ServicioAdicionalRepositoryEF>();
        services.AddScoped<IServicioTemporadaRepository, ServicioTemporadaRepositoryEF>();
        services.AddScoped<ITarifaTemporadaRepository, TarifaTemporadaRepositoryEF>();
        services.AddScoped<ITemporadaRepository, TemporadaRepositoryEF>();
        services.AddScoped<IUsuarioRepository, UsuarioRepositoryEF>();

        // 5) Aquí luego agregas EmailSes / StorageS3
        // services.AddScoped<IEmailSender, SesEmailSender>();
        // services.AddScoped<IFileStorage, S3FileStorage>();

        return services;
    }
}

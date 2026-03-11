using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGRH.Domain.Abstractions.Email;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Abstractions.Services.Time;
using SGRH.Domain.Abstractions.Storage;
using SGRH.Infrastructure.EmailSES;
using SGRH.Infrastructure.Services;
using SGRH.Infrastructure.StorageS3;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories;
using SGRH.Persistence.UnitOfWork;
using SesV2 = Amazon.SimpleEmailV2;

namespace SGRH.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddPersistence(config)
            .AddStorageS3(config)
            .AddEmailSes(config)
            .AddDomainServices();

        return services;
    }

    // ── 1. Persistence ────────────────────────────────────────────────────

    private static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' no encontrada en la configuración.");

        services.AddDbContext<SGRHDbContext>(opt =>
            opt.UseSqlServer(cs));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuditoriaRepository, AuditoriaRepositoryEF>();
        services.AddScoped<ICategoriaHabitacionRepository, CategoriaHabitacionRepositoryEF>();
        services.AddScoped<IClienteRepository, ClienteRepositoryEF>();
        services.AddScoped<IHabitacionRepository, HabitacionRepositoryEF>();
        services.AddScoped<IHabitacionHistorialRepository, HabitacionHistorialRepositoryEF>();
        services.AddScoped<IReservaRepository, ReservaRepositoryEF>();
        services.AddScoped<IServicioAdicionalRepository, ServicioAdicionalRepositoryEF>();
        services.AddScoped<IServicioCategoriaPrecioRepository, ServicioCategoriaPrecioRepositoryEF>();
        services.AddScoped<ITarifaTemporadaRepository, TarifaTemporadaRepositoryEF>();
        services.AddScoped<ITemporadaRepository, TemporadaRepositoryEF>();
        services.AddScoped<IUsuarioRepository, UsuarioRepositoryEF>();

        return services;
    }

    // ── 2. Storage S3 ─────────────────────────────────────────────────────

    private static IServiceCollection AddStorageS3(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<S3Options>(opt =>
            config.GetSection(S3Options.Section).Bind(opt));

        var region = config[$"{S3Options.Section}:Region"]
            ?? throw new InvalidOperationException("AWS:S3:Region no configurado.");
        var accessKey = config[$"{S3Options.Section}:AccessKey"]
            ?? throw new InvalidOperationException("AWS:S3:AccessKey no configurado.");
        var secretKey = config[$"{S3Options.Section}:SecretKey"]
            ?? throw new InvalidOperationException("AWS:S3:SecretKey no configurado.");

        services.AddSingleton<IAmazonS3>(_ =>
            new AmazonS3Client(
                new BasicAWSCredentials(accessKey, secretKey),
                RegionEndpoint.GetBySystemName(region)));

        services.AddScoped<IFileStorage, S3FileStorage>();

        return services;
    }

    // ── 3. Email SES ──────────────────────────────────────────────────────

    private static IServiceCollection AddEmailSes(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<SesOptions>(opt =>
            config.GetSection(SesOptions.Section).Bind(opt));

        var region = config[$"{SesOptions.Section}:Region"]
            ?? throw new InvalidOperationException("AWS:SES:Region no configurado.");
        var accessKey = config[$"{SesOptions.Section}:AccessKey"]
            ?? throw new InvalidOperationException("AWS:SES:AccessKey no configurado.");
        var secretKey = config[$"{SesOptions.Section}:SecretKey"]
            ?? throw new InvalidOperationException("AWS:SES:SecretKey no configurado.");

        services.AddSingleton<SesV2.IAmazonSimpleEmailServiceV2>(_ =>
            new SesV2.AmazonSimpleEmailServiceV2Client(
                new BasicAWSCredentials(accessKey, secretKey),
                RegionEndpoint.GetBySystemName(region)));

        services.AddScoped<IEmailSender, SesEmailSender>();
        services.AddScoped<IAdminNotifier, SesAdminNotifier>();

        return services;
    }

    // ── 4. Domain Services ────────────────────────────────────────────────

    private static IServiceCollection AddDomainServices(
        this IServiceCollection services)
    {
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddScoped<IReservaDomainPolicy, ReservaDomainPolicy>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();

        return services;
    }
}
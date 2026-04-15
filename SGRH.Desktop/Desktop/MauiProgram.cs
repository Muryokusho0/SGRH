using Desktop.Auth;
using Desktop.Services.Auth;
using Desktop.Services.Categorias;
using Desktop.Services.Clientes;
using Desktop.Services.Habitaciones;
using Desktop.Services.Reportes;
using Desktop.Services.Reservas;
using Desktop.Services.Servicios;
using Desktop.Services.Temporadas;
using Desktop.Services.Usuarios;
using Microsoft.Extensions.Logging;

namespace Desktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ── Caché en memoria ──────────────────────────────────────────────
        builder.Services.AddMemoryCache();

        // ── HttpClient hacia la API ───────────────────────────────────────
        builder.Services.AddHttpClient("SGRH.Api", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5216/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "SGRH.Desktop/1.0 MAUI-Windows");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── Sesión (Singleton — una instancia por proceso de la app) ──────
        builder.Services.AddSingleton<TokenStorageService>();

        // ── Servicios (Transient — sin estado compartido entre páginas) ───
        builder.Services.AddTransient<IAuthService, AuthService>();
        builder.Services.AddTransient<IReservaService, ReservaService>();
        builder.Services.AddTransient<IHabitacionService, HabitacionService>();
        builder.Services.AddTransient<IClienteService, ClienteService>();
        builder.Services.AddTransient<IUsuarioService, UsuarioService>();
        builder.Services.AddTransient<IReporteService, ReporteService>();
        builder.Services.AddTransient<ServiciosService>();
        builder.Services.AddTransient<TemporadasService>();
        builder.Services.AddTransient<CategoriasService>();

        return builder.Build();
    }
}
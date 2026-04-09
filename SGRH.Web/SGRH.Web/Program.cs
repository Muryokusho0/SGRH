using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using SGRH.Web.Auth;
using SGRH.Web.Components;
using SGRH.Web.Services.Auth;
using SGRH.Web.Services.Categorias;
using SGRH.Web.Services.Habitaciones;
using SGRH.Web.Services.Reservas;
using SGRH.Web.Services.Servicios;
using SGRH.Web.Services.Temporadas;

namespace SGRH.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // ── Autenticación y autorización ──────────────────────────────────
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
            });

        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<TokenStorageService>();
        builder.Services.AddScoped<AuthenticationStateProvider,
            JwtAuthenticationStateProvider>();

        // ── Caché en memoria ──────────────────────────────────────────────
        // Usado por CategoriaService y TemporadaService para evitar consultas
        // repetitivas a la API con datos que raramente cambian.
        // IMemoryCache es Singleton — una instancia compartida por todos los circuitos.
        builder.Services.AddMemoryCache();

        // ── IHttpClientFactory ────────────────────────────────────────────
        // Sin DelegatingHandler: en Blazor Server IHttpClientFactory resuelve
        // los handlers desde el scope raíz, no desde el circuito.
        // Cada servicio inyecta TokenStorageService directamente (ApiServiceBase).
        builder.Services.AddHttpClient("SGRH.Api", client =>
        {
            var baseUrl = builder.Configuration["ApiBaseUrl"]
                ?? throw new InvalidOperationException(
                    "ApiBaseUrl no configurado en appsettings.json.");

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // La API valida User-Agent en AuditInfo para todos los endpoints.
            client.DefaultRequestHeaders.Add(
                "User-Agent", "SGRH.Web/1.0 Blazor-Server");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── Servicios de aplicación (Scoped = un scope por circuito) ──────
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IReservaService, ReservaService>();
        builder.Services.AddScoped<IHabitacionService, HabitacionService>();
        builder.Services.AddScoped<IServicioService, ServicioService>();
        builder.Services.AddScoped<ICategoriaService, CategoriaService>();
        builder.Services.AddScoped<ITemporadaService, TemporadaService>();

        // ── Pipeline HTTP ─────────────────────────────────────────────────
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
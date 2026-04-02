using Microsoft.Extensions.DependencyInjection;
using SGRH.Application.Abstractions;
using SGRH.Application.UseCases.Auditoria;
using SGRH.Application.UseCases.Auth.Login;
using SGRH.Application.UseCases.Auth.Register;
using SGRH.Application.UseCases.Categorias.CrearCategoria;
using SGRH.Application.UseCases.Categorias.GetCategoria;
using SGRH.Application.UseCases.Categorias.ListarCategorias;
using SGRH.Application.UseCases.Categorias.ModificarCategoria;
using SGRH.Application.UseCases.Clientes.GetCliente;
using SGRH.Application.UseCases.Clientes.ListarClientes;
using SGRH.Application.UseCases.Clientes.ModificarCliente;
using SGRH.Application.UseCases.Habitaciones.BloquearHabitacion;
using SGRH.Application.UseCases.Habitaciones.CambiarEstadoHabitacion;
using SGRH.Application.UseCases.Habitaciones.CrearHabitacion;
using SGRH.Application.UseCases.Habitaciones.GetHabitacion;
using SGRH.Application.UseCases.Habitaciones.GetHistorialHabitacion;
using SGRH.Application.UseCases.Habitaciones.ListarHabitaciones;
using SGRH.Application.UseCases.Habitaciones.ListarHabitacionesDisponibles;
using SGRH.Application.UseCases.Reportes;
using SGRH.Application.UseCases.Reservas.AgregarHabitacion;
using SGRH.Application.UseCases.Reservas.AgregarServicio;
using SGRH.Application.UseCases.Reservas.CambiarFechas;
using SGRH.Application.UseCases.Reservas.CancelarReserva;
using SGRH.Application.UseCases.Reservas.ConfirmarReserva;
using SGRH.Application.UseCases.Reservas.CrearReserva;
using SGRH.Application.UseCases.Reservas.FinalizarReserva;
using SGRH.Application.UseCases.Reservas.GetReserva;
using SGRH.Application.UseCases.Reservas.ListarReservas;
using SGRH.Application.UseCases.Reservas.QuitarHabitacion;
using SGRH.Application.UseCases.Reservas.QuitarServicio;
using SGRH.Application.UseCases.Servicios.CrearServicio;
using SGRH.Application.UseCases.Servicios.GetServicio;
using SGRH.Application.UseCases.Servicios.ListarServicios;
using SGRH.Application.UseCases.Servicios.ListarServiciosPorCategoria;
using SGRH.Application.UseCases.Servicios.AsignarPrecioCategoria;
using SGRH.Application.UseCases.Servicios.AsignarServicioTemporada;
using SGRH.Application.UseCases.Tarifas.CrearTarifa;
using SGRH.Application.UseCases.Tarifas.GetTarifa;
using SGRH.Application.UseCases.Tarifas.ListarTarifas;
using SGRH.Application.UseCases.Temporadas.CrearTemporada;
using SGRH.Application.UseCases.Temporadas.GetTemporada;
using SGRH.Application.UseCases.Temporadas.GetTemporadaVigente;
using SGRH.Application.UseCases.Temporadas.ListarTemporadas;
using SGRH.Application.UseCases.Usuarios.CrearUsuario;
using SGRH.Application.UseCases.Usuarios.DesactivarUsuario;
using SGRH.Application.UseCases.Usuarios.GetUsuario;
using SGRH.Application.UseCases.Usuarios.ListarUsuarios;

namespace SGRH.Application.DependencyInjection;

/// <summary>
/// Registro centralizado de casos de uso y validadores de la capa Application.
/// </summary>
public static class ApplicationDependencyInjection
{
    /// <summary>Agrega todos los casos de uso de la aplicación al contenedor DI.</summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>La misma colección para encadenamiento.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddAuthUseCases()
            .AddUsuarioUseCases()
            .AddClienteUseCases()
            .AddCategoriaUseCases()
            .AddHabitacionUseCases()
            .AddTemporadaUseCases()
            .AddTarifaUseCases()
            .AddServicioUseCases()
            .AddReservaUseCases()
            .AddAuditoriaUseCases()
            .AddReporteUseCases();

        return services;
    }

    // ── Auth ──────────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de autenticación.</summary>
    private static IServiceCollection AddAuthUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<LoginRequest>, LoginValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterValidator>();

        services.AddScoped<LoginUseCase>();
        services.AddScoped<RegisterUseCase>();
        return services;
    }

    // ── Usuarios ──────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de usuarios.</summary>
    private static IServiceCollection AddUsuarioUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CrearUsuarioRequest>, CrearUsuarioValidator>();

        services.AddScoped<CrearUsuarioUseCase>();
        services.AddScoped<DesactivarUsuarioUseCase>();
        services.AddScoped<GetUsuarioUseCase>();
        services.AddScoped<ListarUsuariosUseCase>();
        return services;
    }

    // ── Clientes ──────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de clientes.</summary>
    private static IServiceCollection AddClienteUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ModificarClienteRequest>, ModificarClienteValidator>();

        services.AddScoped<ModificarClienteUseCase>();
        services.AddScoped<GetClienteUseCase>();
        services.AddScoped<ListarClientesUseCase>();
        return services;
    }

    // ── Categorías ────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de categorías.</summary>
    private static IServiceCollection AddCategoriaUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CrearCategoriaRequest>, CrearCategoriaValidator>();
        services.AddScoped<IValidator<ModificarCategoriaRequest>, ModificarCategoriaValidator>();

        services.AddScoped<CrearCategoriaUseCase>();
        services.AddScoped<ModificarCategoriaUseCase>();
        services.AddScoped<GetCategoriaUseCase>();
        services.AddScoped<ListarCategoriasUseCase>();
        return services;
    }

    // ── Habitaciones ──────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de habitaciones.</summary>
    private static IServiceCollection AddHabitacionUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CrearHabitacionRequest>, CrearHabitacionValidator>();
        services.AddScoped<IValidator<BloquearHabitacionRequest>, BloquearHabitacionValidator>();
        services.AddScoped<IValidator<CambiarEstadoHabitacionRequest>, CambiarEstadoHabitacionValidator>();
        services.AddScoped<IValidator<ListarHabitacionesDisponiblesRequest>, ListarHabitacionesDisponiblesValidator>();

        services.AddScoped<CrearHabitacionUseCase>();
        services.AddScoped<BloquearHabitacionUseCase>();
        services.AddScoped<CambiarEstadoHabitacionUseCase>();
        services.AddScoped<GetHabitacionUseCase>();
        services.AddScoped<GetHistorialHabitacionUseCase>();
        services.AddScoped<ListarHabitacionesUseCase>();
        services.AddScoped<ListarHabitacionesDisponiblesUseCase>();
        return services;
    }

    // ── Temporadas ────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de temporadas.</summary>
    private static IServiceCollection AddTemporadaUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CrearTemporadaRequest>, CrearTemporadaValidator>();

        services.AddScoped<CrearTemporadaUseCase>();
        services.AddScoped<GetTemporadaUseCase>();
        services.AddScoped<GetTemporadaVigenteUseCase>();
        services.AddScoped<ListarTemporadasUseCase>();
        return services;
    }

    // ── Tarifas ───────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de tarifas.</summary>
    private static IServiceCollection AddTarifaUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CrearTarifaRequest>, CrearTarifaValidator>();

        services.AddScoped<CrearTarifaUseCase>();
        services.AddScoped<GetTarifaUseCase>();
        services.AddScoped<ListarTarifasUseCase>();
        return services;
    }

    // ── Servicios ─────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de servicios.</summary>
    private static IServiceCollection AddServicioUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CrearServicioRequest>, CrearServicioValidator>();
        services.AddScoped<IValidator<AsignarPrecioCategoriaRequest>, AsignarPrecioCategoriaValidator>();
        services.AddScoped<IValidator<AsignarServicioTemporadaRequest>, AsignarServicioTemporadaValidator>();

        services.AddScoped<CrearServicioUseCase>();
        services.AddScoped<AsignarPrecioCategoriaUseCase>();
        services.AddScoped<AsignarServicioTemporadaUseCase>();
        services.AddScoped<GetServicioUseCase>();
        services.AddScoped<ListarServiciosUseCase>();
        services.AddScoped<ListarServiciosPorCategoriaUseCase>();
        return services;
    }

    // ── Reservas ──────────────────────────────────────────────────────────
    /// <summary>Registra validadores y casos de uso de reservas.</summary>
    private static IServiceCollection AddReservaUseCases(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CrearReservaRequest>, CrearReservaValidator>();
        services.AddScoped<IValidator<AgregarHabitacionRequest>, AgregarHabitacionValidator>();
        services.AddScoped<IValidator<AgregarServicioRequest>, AgregarServicioValidator>();
        services.AddScoped<IValidator<CambiarFechasRequest>, CambiarFechasValidator>();

        services.AddScoped<CrearReservaUseCase>();
        services.AddScoped<AgregarHabitacionUseCase>();
        services.AddScoped<QuitarHabitacionUseCase>();
        services.AddScoped<AgregarServicioUseCase>();
        services.AddScoped<QuitarServicioUseCase>();
        services.AddScoped<CambiarFechasUseCase>();
        services.AddScoped<ConfirmarReservaUseCase>();
        services.AddScoped<CancelarReservaUseCase>();
        services.AddScoped<FinalizarReservaUseCase>();
        services.AddScoped<GetReservaUseCase>();
        services.AddScoped<ListarReservasUseCase>();
        return services;
    }

    // ── Auditoría ─────────────────────────────────────────────────────────
    /// <summary>Registra casos de uso de auditoría.</summary>
    private static IServiceCollection AddAuditoriaUseCases(this IServiceCollection services)
    {
        services.AddScoped<ListarAuditoriaUseCase>();
        return services;
    }

    // ── Reportes ──────────────────────────────────────────────────────────
    /// <summary>Registra casos de uso de reportes.</summary>
    private static IServiceCollection AddReporteUseCases(this IServiceCollection services)
    {
        services.AddScoped<ListarReportesUseCase>();
        return services;
    }
}
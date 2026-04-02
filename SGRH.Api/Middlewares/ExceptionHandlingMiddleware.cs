using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Exceptions;
using System.Text.Json;

namespace SGRH.Api.Middleware;

/// <summary>
/// Middleware global de excepciones que transforma errores en JSON consistente.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Inicializa el middleware con el siguiente delegado y logger.</summary>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Ejecuta el siguiente middleware y captura excepciones.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    /// <summary>Convierte una excepción en respuesta HTTP con formato estándar.</summary>
    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, errors) = ex switch
        {
            // ── 400 — Validación de request ───────────────────────────────
            ApplicationValidationException e =>
                (400, "Errores de validación.", e.Errors),

            // ── 401 — Credenciales inválidas ──────────────────────────────
            UnauthorizedException e =>
                (401, e.Message, EmptyErrors()),

            // ── 404 — Entidad no encontrada ───────────────────────────────
            NotFoundException e =>
                (404, e.Message, EmptyErrors()),

            // ── 409 — Conflicto de unicidad ───────────────────────────────
            ConflictException e =>
                (409, e.Message, EmptyErrors()),

            // ── 422 — Regla de negocio violada ────────────────────────────
            BusinessRuleViolationException e =>
                (422, e.Message, EmptyErrors()),

            // ── 500 — Cualquier otra excepción ────────────────────────────
            _ =>
                (500, "Ha ocurrido un error interno. Por favor intente de nuevo.", EmptyErrors())
        };

        // Solo los 500 se loguean — los errores de negocio son esperados
        if (status == 500)
            _logger.LogError(ex,
                "Unhandled exception en {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new ErrorResponse(status, title, errors), _jsonOptions);
        await context.Response.WriteAsync(body);
    }

    /// <summary>Devuelve una lista vacía de errores.</summary>
    private static IReadOnlyList<string> EmptyErrors() => [];
}

/// <summary>Modelo de error serializado en las respuestas del middleware.</summary>
internal sealed record ErrorResponse(
    int Status,
    string Title,
    IReadOnlyList<string> Errors);
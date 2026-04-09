using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Exceptions;
using SGRH.Persistence.Exceptions;
using System.Text.Json;

namespace SGRH.Api.Middleware;

/// <summary>
/// Middleware global de excepciones que transforma errores en JSON consistente.
///
/// Maneja adicionalmente <see cref="PersistenceException"/> lanzadas por la
/// capa de persistencia, mapeando cada <see cref="PersistenceErrorType"/>
/// al código HTTP apropiado.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

            // ── 409 — Conflicto de unicidad o concurrencia ────────────────
            ConflictException e =>
                (409, e.Message, EmptyErrors()),

            // ── 409 / 400 / 500 — Excepciones de persistencia ────────────
            // PersistenceException puede indicar distintos tipos de error
            // según su PersistenceErrorType. Se mapea cada tipo al código
            // HTTP más apropiado para comunicar correctamente al cliente.
            PersistenceException e =>
                MapPersistenceException(e),

            // ── 422 — Regla de negocio violada ────────────────────────────
            BusinessRuleViolationException e =>
                (422, e.Message, EmptyErrors()),

            // ── 500 — Cualquier otra excepción no anticipada ──────────────
            _ =>
                (500, "Ha ocurrido un error interno. Por favor intente de nuevo.",
                 EmptyErrors())
        };

        // Solo los errores 5xx se loguean como Error — los errores de negocio
        // (4xx) son flujos esperados y se loguean como Warning.
        if (status >= 500)
            _logger.LogError(ex,
                "Unhandled exception en {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        else if (status == 409 || status == 422)
            _logger.LogWarning(ex,
                "Business/persistence exception en {Method} {Path}: {Title}",
                context.Request.Method,
                context.Request.Path,
                title);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(
            new ErrorResponse(status, title, errors), _jsonOptions);
        await context.Response.WriteAsync(body);
    }

    /// <summary>
    /// Mapea el tipo de error de persistencia al código HTTP y mensaje apropiados.
    /// </summary>
    private static (int Status, string Title, IReadOnlyList<string> Errors)
        MapPersistenceException(PersistenceException ex)
        => ex.ErrorType switch
        {
            // Restricción única → 409 Conflict
            PersistenceErrorType.UniqueConstraintViolation =>
                (409, ex.Message, EmptyErrors()),

            // Restricción de FK → 400 Bad Request
            // (el cliente envió datos que referencian algo que no existe)
            PersistenceErrorType.ForeignKeyViolation =>
                (400, ex.Message, EmptyErrors()),

            // Conflicto de concurrencia → 409 Conflict
            PersistenceErrorType.ConcurrencyConflict =>
                (409, ex.Message, EmptyErrors()),

            // Error general de BD → 500 Internal Server Error
            _ =>
                (500, "Error al procesar la operación en la base de datos.", EmptyErrors())
        };

    private static IReadOnlyList<string> EmptyErrors() => [];
}

/// <summary>Modelo de error serializado en las respuestas del middleware.</summary>
internal sealed record ErrorResponse(
    int Status,
    string Title,
    IReadOnlyList<string> Errors);
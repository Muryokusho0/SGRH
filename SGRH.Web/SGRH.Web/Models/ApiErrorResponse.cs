namespace SGRH.Web.Models;

/// <summary>
/// Mapea la respuesta de error que devuelve el middleware de SGRH.Api.
/// Formato real: { "status": 422, "title": "...", "errors": [] }
/// </summary>
public sealed class ApiErrorResponse
{
    public int Status { get; init; }
    public string? Title { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Desktop.Helpers;

public static class ApiErrorHelper
{
    public static async Task<string> LeerErrorAsync(HttpResponseMessage resp)
    {
        try
        {
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            if (json.TryGetProperty("title", out var title) && title.GetString() is { } t)
                return t;
            if (json.TryGetProperty("error", out var error) && error.GetString() is { } e)
                return e;
            if (json.TryGetProperty("errors", out var errors))
            {
                var msgs = errors.EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => x != null)
                    .ToList();
                if (msgs.Count > 0)
                    return string.Join(" • ", msgs);
            }
        }
        catch { }

        return resp.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Credenciales inválidas o sesión expirada.",
            System.Net.HttpStatusCode.Forbidden => "No tienes permiso para realizar esta acción.",
            System.Net.HttpStatusCode.NotFound => "El recurso solicitado no fue encontrado.",
            System.Net.HttpStatusCode.Conflict => "Conflicto: ya existe un registro con esos datos.",
            System.Net.HttpStatusCode.UnprocessableEntity => "Los datos no cumplen las reglas de negocio.",
            System.Net.HttpStatusCode.InternalServerError => "Error interno del servidor.",
            _ => $"Error {(int)resp.StatusCode}."
        };
    }

    public static string TraducirExcepcion(Exception ex) => ex switch
    {
        TaskCanceledException => "La solicitud tardó demasiado. Verifica tu conexión.",
        HttpRequestException => "No se pudo conectar con el servidor.",
        _ => "Error inesperado. Intenta de nuevo."
    };
}
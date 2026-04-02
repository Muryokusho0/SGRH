using System.Text.Json;
using System.Text.Json.Serialization;

namespace SGRH.Api.Converters;

/// <summary>
/// Convierte <see cref="DateTime"/> a string con offset local (UTC-4) y viceversa.
/// </summary>
public sealed class DateTimeLocalConverter : JsonConverter<DateTime>
{
    private static readonly TimeZoneInfo _zona =
        TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");

    /// <summary>Lee un DateTime desde JSON.</summary>
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Al leer, parsear el string y devolver como Unspecified
        return DateTime.Parse(reader.GetString()!);
    }

    /// <summary>Escribe un DateTime en JSON con offset local.</summary>
    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        // Convertir a UTC-4 y serializar con offset explícito
        var local = TimeZoneInfo.ConvertTimeFromUtc(
            value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
            _zona);

        var offset = _zona.GetUtcOffset(local);
        var dto = new DateTimeOffset(local, offset);

        // Formato: 2025-04-01T14:30:00-04:00
        writer.WriteStringValue(dto.ToString("yyyy-MM-ddTHH:mm:sszzz"));
    }
}
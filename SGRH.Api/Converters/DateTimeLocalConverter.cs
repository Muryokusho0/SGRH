using System.Text.Json;
using System.Text.Json.Serialization;

namespace SGRH.Api.Converters;

/// <summary>
/// Serializa DateTime como string con offset -04:00 (UTC-4, República Dominicana).
/// Evita que Swagger y otros clientes JSON interpreten las fechas como UTC
/// y las muestren con 4 horas de diferencia.
/// </summary>
public sealed class DateTimeLocalConverter : JsonConverter<DateTime>
{
    private static readonly TimeZoneInfo _zona =
        TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");

    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Al leer, parsear el string y devolver como Unspecified
        return DateTime.Parse(reader.GetString()!);
    }

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
namespace SGRH.Domain.Common;
public static class HoraLocal
{
    private static readonly TimeZoneInfo _zona =
        TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");

    /// <summary>Hora actual en la zona horaria local (UTC-4).</summary>
    public static DateTime Ahora =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zona);
}
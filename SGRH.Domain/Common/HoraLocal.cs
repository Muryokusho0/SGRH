namespace SGRH.Domain.Common;

/// <summary>
/// Utilidad estática que provee la hora actual en la zona horaria local del sistema
/// (UTC-4, correspondiente a la zona horaria del Caribe y Venezuela).
/// </summary>
/// <remarks>
/// Se utiliza para registrar fechas en entidades de dominio como
/// <c>AuditoriaEvento</c>, <c>HabitacionHistorial</c> y <c>Reserva</c>
/// en lugar de <c>DateTime.UtcNow</c>, de modo que las horas almacenadas
/// reflejen la hora local del negocio.
/// </remarks>
public static class HoraLocal
{
    private static readonly TimeZoneInfo _zona =
        TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");

    /// <summary>
    /// Obtiene la hora actual convertida a la zona horaria local (UTC-4).
    /// </summary>
    public static DateTime Ahora =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zona);
}
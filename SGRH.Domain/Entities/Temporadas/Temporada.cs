using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Temporadas;

/// <summary>
/// Representa un período de temporada del hotel que puede ser de dos tipos:
/// <list type="bullet">
///   <item><description><b>Específico:</b> con fechas concretas (año, mes, día).</description></item>
///   <item><description><b>Recurrente:</b> definido por mes y día, repitiéndose cada año (por ejemplo, Navidad del 15-dic al 14-ene).</description></item>
/// </list>
/// Las temporadas se usan para determinar tarifas de habitaciones y disponibilidad de servicios adicionales.
/// </summary>
public sealed class Temporada : EntityBase
{
    /// <summary>
    /// Identificador único de la temporada.
    /// </summary>
    public int TemporadaId { get; private set; }

    /// <summary>
    /// Nombre descriptivo de la temporada (por ejemplo: "Temporada Alta Navideña", "Verano").
    /// </summary>
    public string NombreTemporada { get; private set; } = default!;

    /// <summary>
    /// Fecha de inicio de la temporada específica (con año). Solo aplica si <see cref="EsRecurrente"/> es <c>false</c>.
    /// </summary>
    public DateTime? FechaInicio { get; private set; }

    /// <summary>
    /// Fecha de fin de la temporada específica (con año). Solo aplica si <see cref="EsRecurrente"/> es <c>false</c>.
    /// </summary>
    public DateTime? FechaFin { get; private set; }

    /// <summary>
    /// Indica si la temporada es recurrente (se repite cada año por mes y día)
    /// o específica (tiene año concreto).
    /// </summary>
    public bool EsRecurrente { get; private set; }

    /// <summary>
    /// Mes de inicio del rango recurrente (1-12). Solo aplica si <see cref="EsRecurrente"/> es <c>true</c>.
    /// </summary>
    public byte? MesInicio { get; private set; }

    /// <summary>
    /// Día de inicio del rango recurrente (1-31). Solo aplica si <see cref="EsRecurrente"/> es <c>true</c>.
    /// </summary>
    public byte? DiaInicio { get; private set; }

    /// <summary>
    /// Mes de fin del rango recurrente (1-12). Solo aplica si <see cref="EsRecurrente"/> es <c>true</c>.
    /// </summary>
    public byte? MesFin { get; private set; }

    /// <summary>
    /// Día de fin del rango recurrente (1-31). Solo aplica si <see cref="EsRecurrente"/> es <c>true</c>.
    /// </summary>
    public byte? DiaFin { get; private set; }

    private Temporada() { }

    /// <summary>
    /// Crea una temporada específica con fechas concretas (incluyendo año).
    /// </summary>
    /// <param name="nombreTemporada">Nombre de la temporada (máx. 50 caracteres).</param>
    /// <param name="fechaInicio">Fecha de inicio (debe ser anterior a <paramref name="fechaFin"/>).</param>
    /// <param name="fechaFin">Fecha de fin.</param>
    public Temporada(string nombreTemporada, DateTime fechaInicio, DateTime fechaFin)
    {
        Guard.AgainstNullOrWhiteSpace(nombreTemporada, nameof(nombreTemporada), 50);
        Guard.AgainstInvalidDateRange(fechaInicio, fechaFin,
            nameof(fechaInicio), nameof(fechaFin));

        NombreTemporada = nombreTemporada;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        EsRecurrente = false;
    }

    /// <summary>
    /// Crea una temporada recurrente definida por mes y día, que se repite cada año.
    /// </summary>
    /// <param name="nombreTemporada">Nombre de la temporada (máx. 50 caracteres).</param>
    /// <param name="mesInicio">Mes de inicio del rango (1-12).</param>
    /// <param name="diaInicio">Día de inicio del rango (1-31).</param>
    /// <param name="mesFin">Mes de fin del rango (1-12).</param>
    /// <param name="diaFin">Día de fin del rango (1-31).</param>
    public Temporada(
        string nombreTemporada,
        byte mesInicio, byte diaInicio,
        byte mesFin, byte diaFin)
    {
        Guard.AgainstNullOrWhiteSpace(nombreTemporada, nameof(nombreTemporada), 50);
        ValidarMesDia(mesInicio, diaInicio, nameof(mesInicio));
        ValidarMesDia(mesFin, diaFin, nameof(mesFin));

        NombreTemporada = nombreTemporada;
        EsRecurrente = true;
        MesInicio = mesInicio;
        DiaInicio = diaInicio;
        MesFin = mesFin;
        DiaFin = diaFin;
    }

    /// <summary>
    /// Actualiza los datos de una temporada específica (con año). No puede usarse en temporadas recurrentes.
    /// </summary>
    /// <param name="nombreTemporada">Nuevo nombre de la temporada (máx. 50 caracteres).</param>
    /// <param name="fechaInicio">Nueva fecha de inicio.</param>
    /// <param name="fechaFin">Nueva fecha de fin.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la temporada es recurrente.</exception>
    public void Actualizar(string nombreTemporada, DateTime fechaInicio, DateTime fechaFin)
    {
        if (EsRecurrente)
            throw new BusinessRuleViolationException(
                "Esta es una temporada recurrente. Usa ActualizarRecurrente para modificarla.");

        Guard.AgainstNullOrWhiteSpace(nombreTemporada, nameof(nombreTemporada), 50);
        Guard.AgainstInvalidDateRange(fechaInicio, fechaFin,
            nameof(fechaInicio), nameof(fechaFin));

        NombreTemporada = nombreTemporada;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    /// <summary>
    /// Actualiza los datos de una temporada recurrente. No puede usarse en temporadas específicas.
    /// </summary>
    /// <param name="nombreTemporada">Nuevo nombre de la temporada (máx. 50 caracteres).</param>
    /// <param name="mesInicio">Nuevo mes de inicio del rango recurrente (1-12).</param>
    /// <param name="diaInicio">Nuevo día de inicio (1-31).</param>
    /// <param name="mesFin">Nuevo mes de fin del rango recurrente (1-12).</param>
    /// <param name="diaFin">Nuevo día de fin (1-31).</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si la temporada es específica (no recurrente).</exception>
    public void ActualizarRecurrente(
        string nombreTemporada,
        byte mesInicio, byte diaInicio,
        byte mesFin, byte diaFin)
    {
        if (!EsRecurrente)
            throw new BusinessRuleViolationException(
                "Esta es una temporada específica. Usa Actualizar para modificarla.");

        Guard.AgainstNullOrWhiteSpace(nombreTemporada, nameof(nombreTemporada), 50);
        ValidarMesDia(mesInicio, diaInicio, nameof(mesInicio));
        ValidarMesDia(mesFin, diaFin, nameof(mesFin));

        NombreTemporada = nombreTemporada;
        MesInicio = mesInicio;
        DiaInicio = diaInicio;
        MesFin = mesFin;
        DiaFin = diaFin;
    }

    /// <summary>
    /// Determina si una fecha específica cae dentro del rango de esta temporada.
    /// Para temporadas recurrentes, compara mes y día ignorando el año.
    /// Soporta rangos que cruzan el fin de año (por ejemplo, del 15-dic al 14-ene).
    /// </summary>
    /// <param name="fecha">Fecha a evaluar.</param>
    /// <returns><c>true</c> si la fecha cae dentro del rango de la temporada; de lo contrario, <c>false</c>.</returns>
    public bool Contiene(DateTime fecha)
    {
        if (!EsRecurrente)
        {
            return FechaInicio.HasValue && FechaFin.HasValue
                && fecha >= FechaInicio.Value
                && fecha < FechaFin.Value;
        }

        // Recurrente: compara mes y día ignorando el año
        var mes = (byte)fecha.Month;
        var dia = (byte)fecha.Day;

        if (MesInicio!.Value < MesFin!.Value ||
           (MesInicio.Value == MesFin.Value && DiaInicio!.Value <= DiaFin!.Value))
        {
            // Rango normal: ej. 15-mar al 14-jun
            return (mes > MesInicio.Value || (mes == MesInicio.Value && dia >= DiaInicio!.Value))
                && (mes < MesFin.Value || (mes == MesFin.Value && dia < DiaFin!.Value));
        }
        else
        {
            // Rango cruza año: ej. 15-dic al 14-ene
            return (mes > MesInicio.Value || (mes == MesInicio.Value && dia >= DiaInicio!.Value))
                || (mes < MesFin.Value || (mes == MesFin.Value && dia < DiaFin!.Value));
        }
    }

    /// <summary>
    /// Valida que el mes esté entre 1 y 12, y el día entre 1 y 31.
    /// </summary>
    /// <param name="mes">Número de mes a validar.</param>
    /// <param name="dia">Número de día a validar.</param>
    /// <param name="nombre">Nombre del campo, usado en el mensaje de error.</param>
    /// <exception cref="Exceptions.ValidationException">Si el mes o el día están fuera del rango válido.</exception>
    private static void ValidarMesDia(byte mes, byte dia, string nombre)
    {
        if (mes < 1 || mes > 12)
            throw new ValidationException($"{nombre}: mes debe estar entre 1 y 12.");
        if (dia < 1 || dia > 31)
            throw new ValidationException($"{nombre}: día debe estar entre 1 y 31.");
    }

    protected override object GetKey() => TemporadaId;
}
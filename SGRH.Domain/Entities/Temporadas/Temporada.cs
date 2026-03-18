using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Temporadas;

public sealed class Temporada : EntityBase
{
    public int TemporadaId { get; private set; }
    public string NombreTemporada { get; private set; } = default!;

    // ── Modo específico (EsRecurrente = false) ──
    // FechaInicio y FechaFin con año concreto
    public DateTime? FechaInicio { get; private set; }
    public DateTime? FechaFin { get; private set; }

    // ── Modo recurrente (EsRecurrente = true) ──
    // El rango se repite cada año por mes y día.
    // Ejemplo: del 15 de diciembre al 14 de enero (navidad).
    public bool EsRecurrente { get; private set; }
    public byte? MesInicio { get; private set; }
    public byte? DiaInicio { get; private set; }
    public byte? MesFin { get; private set; }
    public byte? DiaFin { get; private set; }

    private Temporada() { }

    // Constructor para temporada específica (con año)
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

    // Constructor para temporada recurrente (sin año — se repite cada año)
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

    // Verifica si una fecha cae dentro de esta temporada
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

    private static void ValidarMesDia(byte mes, byte dia, string nombre)
    {
        if (mes < 1 || mes > 12)
            throw new ValidationException($"{nombre}: mes debe estar entre 1 y 12.");
        if (dia < 1 || dia > 31)
            throw new ValidationException($"{nombre}: día debe estar entre 1 y 31.");
    }

    protected override object GetKey() => TemporadaId;
}
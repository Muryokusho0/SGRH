using System;

namespace Desktop.Models.Temporadas;

public sealed class TemporadaResumen
{
    public int TemporadaId { get; init; }
    public string NombreTemporada { get; init; } = string.Empty;
    public DateTime FechaInicio { get; init; }
    public DateTime FechaFin { get; init; }
    public bool EsActual => DateTime.Today >= FechaInicio && DateTime.Today < FechaFin;
}
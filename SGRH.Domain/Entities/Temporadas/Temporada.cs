using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Temporadas;

public sealed class Temporada : EntityBase
{
    public int TemporadaId { get; private set; }
    public string NombreTemporada { get; private set; } = default!;

    public DateTime FechaInicio { get; private set; }

    public DateTime FechaFin { get; private set; }

    private Temporada() { }

    public Temporada(string nombreTemporada, DateTime fechaInicio, DateTime fechaFin)
    {
        Guard.AgainstNullOrWhiteSpace(nombreTemporada, nameof(nombreTemporada), 50);

        Guard.AgainstInvalidDateRange(fechaInicio, fechaFin,
                                      nameof(fechaInicio), nameof(fechaFin));

        NombreTemporada = nombreTemporada;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    public void Actualizar(string nombreTemporada, DateTime fechaInicio, DateTime fechaFin)
    {
        Guard.AgainstNullOrWhiteSpace(nombreTemporada, nameof(nombreTemporada), 50);
        Guard.AgainstInvalidDateRange(fechaInicio, fechaFin,
                                      nameof(fechaInicio), nameof(fechaFin));

        NombreTemporada = nombreTemporada;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    public bool Contiene(DateTime fecha)
        => fecha >= FechaInicio && fecha < FechaFin;

    protected override object GetKey() => TemporadaId;
}

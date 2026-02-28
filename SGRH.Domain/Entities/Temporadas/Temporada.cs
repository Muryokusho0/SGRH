using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Temporadas;

public sealed class Temporada : EntityBase
{
    public int TemporadaId { get; private set; }
    public string NombreTemporada { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime FechaFin { get; private set; } // Exclusiva

    private Temporada() { }

    public Temporada(string nombreTemporada, DateTime fechaInicio, DateTime fechaFin)
    {
        Guard.AgainstNullOrWhiteSpace(nombreTemporada, nameof(nombreTemporada), 50);

        if (fechaInicio >= fechaFin)
            throw new DomainException("FechaInicio debe ser menor que FechaFin (exclusiva).");

        NombreTemporada = nombreTemporada;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    public bool ContieneFecha(DateTime fecha)
    {
        return fecha >= FechaInicio && fecha < FechaFin;
    }

    protected override object GetKey() => TemporadaId;
}

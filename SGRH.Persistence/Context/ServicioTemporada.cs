using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Context;

public sealed class ServicioTemporada
{
    public int ServicioAdicionalId { get; private set; }
    public int TemporadaId { get; private set; }

    private ServicioTemporada() { }

    public ServicioTemporada(int servicioAdicionalId, int temporadaId)
    {
        ServicioAdicionalId = servicioAdicionalId;
        TemporadaId = temporadaId;
    }
}
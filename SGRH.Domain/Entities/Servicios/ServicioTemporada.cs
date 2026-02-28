using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;

namespace SGRH.Domain.Entities.Servicios;

public sealed class ServicioTemporada : EntityBase
{
    public int ServicioAdicionalId { get; private set; }
    public int TemporadaId { get; private set; }

    private ServicioTemporada() { }

    public ServicioTemporada(int servicioAdicionalId, int temporadaId)
    {
        ServicioAdicionalId = servicioAdicionalId;
        TemporadaId = temporadaId;
    }

    protected override object GetKey()
        => $"{ServicioAdicionalId}-{TemporadaId}";
}

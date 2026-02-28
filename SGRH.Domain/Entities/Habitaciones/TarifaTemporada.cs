using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;


namespace SGRH.Domain.Entities.Habitaciones;
public sealed class TarifaTemporada : EntityBase
{
    public int TarifaTemporadaId { get; private set; }
    public int CategoriaHabitacionId { get; private set; }
    public int TemporadaId { get; private set; }
    public decimal Precio { get; private set; }

    private TarifaTemporada() { }

    public TarifaTemporada(
        int categoriaHabitacionId,
        int temporadaId,
        decimal precio)
    {
        if (precio <= 0)
            throw new DomainException("Precio debe ser mayor que cero.");

        CategoriaHabitacionId = categoriaHabitacionId;
        TemporadaId = temporadaId;
        Precio = precio;
    }

    protected override object GetKey() => TarifaTemporadaId;
}

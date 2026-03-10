using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    public TarifaTemporada(int categoriaHabitacionId, int temporadaId, decimal precio)
    {
        Guard.AgainstOutOfRange(categoriaHabitacionId, nameof(categoriaHabitacionId), 0);
        Guard.AgainstOutOfRange(temporadaId, nameof(temporadaId), 0);
        Guard.AgainstOutOfRange(precio, nameof(precio), 0m);

        CategoriaHabitacionId = categoriaHabitacionId;
        TemporadaId = temporadaId;
        Precio = precio;
    }

    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        Guard.AgainstOutOfRange(nuevoPrecio, nameof(nuevoPrecio), 0m);
        Precio = nuevoPrecio;
    }

    protected override object GetKey() => TarifaTemporadaId;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Servicios;

public sealed class ServicioCategoriaPrecio : EntityBase
{
    public int ServicioAdicionalId { get; private set; }
    public int CategoriaHabitacionId { get; private set; }
    public decimal Precio { get; private set; }

    private ServicioCategoriaPrecio() { }

    public ServicioCategoriaPrecio(
        int servicioAdicionalId,
        int categoriaHabitacionId,
        decimal precio)
    {
        Guard.AgainstOutOfRange(servicioAdicionalId, nameof(servicioAdicionalId), 0);
        Guard.AgainstOutOfRange(categoriaHabitacionId, nameof(categoriaHabitacionId), 0);
        Guard.AgainstOutOfRange(precio, nameof(precio), 0m);

        ServicioAdicionalId = servicioAdicionalId;
        CategoriaHabitacionId = categoriaHabitacionId;
        Precio = precio;
    }

    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        Guard.AgainstOutOfRange(nuevoPrecio, nameof(nuevoPrecio), 0m);
        Precio = nuevoPrecio;
    }

    protected override object GetKey() => $"{ServicioAdicionalId}-{CategoriaHabitacionId}";
}
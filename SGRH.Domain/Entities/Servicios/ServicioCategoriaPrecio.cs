using System;
using System.Collections.Generic;
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
        if (precio <= 0)
            throw new DomainException("Precio inválido.");

        ServicioAdicionalId = servicioAdicionalId;
        CategoriaHabitacionId = categoriaHabitacionId;
        Precio = precio;
    }

    protected override object GetKey()
        => $"{ServicioAdicionalId}-{CategoriaHabitacionId}";
}

using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Entities.Habitaciones;

public sealed class CategoriaHabitacion : EntityBase
{
    public int CategoriaHabitacionId { get; private set; }

    public string NombreCategoria { get; private set; } = default!;

    public int Capacidad { get; private set; }

    public string Descripcion { get; private set; } = default!;

    public decimal PrecioBase { get; private set; }

    private CategoriaHabitacion() { }

    public CategoriaHabitacion(
        string nombreCategoria,
        int capacidad,
        string descripcion,
        decimal precioBase)
    {
        Guard.AgainstNullOrWhiteSpace(nombreCategoria, nameof(nombreCategoria), 50);
        Guard.AgainstNullOrWhiteSpace(descripcion, nameof(descripcion), 255);
        Guard.AgainstOutOfRange(capacidad, nameof(capacidad), 0);
        Guard.AgainstOutOfRange(precioBase, nameof(precioBase), 0m);

        NombreCategoria = nombreCategoria;
        Capacidad = capacidad;
        Descripcion = descripcion;
        PrecioBase = precioBase;
    }

    public void Actualizar(
        string nombreCategoria,
        int capacidad,
        string descripcion,
        decimal precioBase)
    {
        Guard.AgainstNullOrWhiteSpace(nombreCategoria, nameof(nombreCategoria), 50);
        Guard.AgainstNullOrWhiteSpace(descripcion, nameof(descripcion), 255);
        Guard.AgainstOutOfRange(capacidad, nameof(capacidad), 0);
        Guard.AgainstOutOfRange(precioBase, nameof(precioBase), 0m);

        NombreCategoria = nombreCategoria;
        Capacidad = capacidad;
        Descripcion = descripcion;
        PrecioBase = precioBase;
    }

    protected override object GetKey() => CategoriaHabitacionId;
}


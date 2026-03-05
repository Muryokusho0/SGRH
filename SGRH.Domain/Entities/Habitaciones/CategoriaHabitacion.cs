using SGRH.Domain.Base;
using SGRH.Domain.Common;
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
    public string NombreCategoria { get; private set; }
    public int Capacidad { get; private set; }
    public string Descripcion { get; private set; }
    public decimal PrecioBase { get; private set; }

    private readonly List<TarifaTemporada> _tarifas = [];
    public IReadOnlyCollection<TarifaTemporada> Tarifas => _tarifas;

    private CategoriaHabitacion() { }

    public CategoriaHabitacion(
        string nombreCategoria,
        int capacidad,
        string descripcion,
        decimal precioBase)
    {
        Guard.AgainstNullOrWhiteSpace(nombreCategoria, nameof(nombreCategoria), 50);
        Guard.AgainstNullOrWhiteSpace(descripcion, nameof(descripcion), 255);

        if (capacidad <= 0)
            throw new ValidationException("Capacidad debe ser mayor que cero.");

        if (precioBase <= 0)
            throw new ValidationException("Precio base debe ser mayor que cero.");

        NombreCategoria = nombreCategoria;
        Capacidad = capacidad;
        Descripcion = descripcion;
        PrecioBase = precioBase;
    }

    public void AgregarTarifaTemporada(TarifaTemporada tarifa)
    {
        _tarifas.Add(tarifa);
    }

    protected override object GetKey() => CategoriaHabitacionId;
}


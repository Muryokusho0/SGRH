using SGRH.Domain.Base;
using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Entities.Servicios;

public sealed class ServicioAdicional : EntityBase
{
    public int ServicioAdicionalId { get; private set; }
    public string NombreServicio { get; private set; }
    public string TipoServicio { get; private set; }

    private readonly List<ServicioCategoriaPrecio> _precios = new();
    private readonly List<ServicioTemporada> _temporadas = new();
    public IReadOnlyCollection<ServicioCategoriaPrecio> Precios => _precios;
    public IReadOnlyCollection<ServicioTemporada> Temporadas => _temporadas;

    public void HabilitarEnTemporada(int temporadaId)
    {
        if (_temporadas.Any(t => t.TemporadaId == temporadaId))
            return;

        _temporadas.Add(new ServicioTemporada(ServicioAdicionalId, temporadaId));
    }

    private ServicioAdicional() { }

    public ServicioAdicional(string nombreServicio, string tipoServicio)
    {
        Guard.AgainstNullOrWhiteSpace(nombreServicio, nameof(nombreServicio), 50);
        Guard.AgainstNullOrWhiteSpace(tipoServicio, nameof(tipoServicio), 50);

        NombreServicio = nombreServicio;
        TipoServicio = tipoServicio;
    }

    public void AgregarPrecio(int categoriaHabitacionId, decimal precio)
    {
        if (precio <= 0)
            throw new DomainException("Precio debe ser mayor que cero.");

        if (_precios.Any(p => p.CategoriaHabitacionId == categoriaHabitacionId))
            throw new DomainException("Ya existe precio para esa categoría.");

        _precios.Add(new ServicioCategoriaPrecio(
            ServicioAdicionalId,
            categoriaHabitacionId,
            precio));
    }

    protected override object GetKey() => ServicioAdicionalId;
}
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Entities.Servicios;

public sealed class ServicioAdicional : EntityBase
{
    public int ServicioAdicionalId { get; private set; }
    public string NombreServicio { get; private set; } = default!;
    public string TipoServicio { get; private set; } = default!;

    private readonly List<int> _temporadaIds = [];
    public IReadOnlyCollection<int> TemporadaIds => _temporadaIds;

    private ServicioAdicional() { }

    public ServicioAdicional(string nombreServicio, string tipoServicio)
    {
        Guard.AgainstNullOrWhiteSpace(nombreServicio, nameof(nombreServicio), 50);
        Guard.AgainstNullOrWhiteSpace(tipoServicio, nameof(tipoServicio), 50);

        NombreServicio = nombreServicio;
        TipoServicio = tipoServicio;
    }

    public void HabilitarEnTemporada(int temporadaId)
    {
        Guard.AgainstOutOfRange(temporadaId, nameof(temporadaId), 0);

        if (_temporadaIds.Contains(temporadaId))
            throw new ConflictException(
                $"El servicio ya está habilitado para la temporada {temporadaId}.");

        _temporadaIds.Add(temporadaId);
    }

    public void DeshabilitarEnTemporada(int temporadaId)
    {
        if (!_temporadaIds.Contains(temporadaId))
            throw new NotFoundException("ServicioTemporada", temporadaId.ToString());

        _temporadaIds.Remove(temporadaId);
    }

    public bool EstaDisponibleEn(int? temporadaId)
    {
        if (temporadaId is null) return true;

        return _temporadaIds.Contains(temporadaId.Value);
    }

    public void Actualizar(string nombreServicio, string tipoServicio)
    {
        Guard.AgainstNullOrWhiteSpace(nombreServicio, nameof(nombreServicio), 50);
        Guard.AgainstNullOrWhiteSpace(tipoServicio, nameof(tipoServicio), 50);

        NombreServicio = nombreServicio;
        TipoServicio = tipoServicio;
    }

    protected override object GetKey() => ServicioAdicionalId;
}
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Servicios;

public sealed class ServicioAdicional : EntityBase
{
    public int ServicioAdicionalId { get; private set; }
    public string NombreServicio { get; private set; } = default!;
    public string TipoServicio { get; private set; } = default!;

    // Si true, el servicio está disponible en TODAS las temporadas
    // sin necesidad de estar explícitamente asignado a cada una.
    public bool AplicaTodasTemporadas { get; private set; }

    private readonly List<int> _temporadaIds = [];
    public IReadOnlyCollection<int> TemporadaIds => _temporadaIds;

    private ServicioAdicional() { }

    public ServicioAdicional(string nombreServicio, string tipoServicio)
    {
        Guard.AgainstNullOrWhiteSpace(nombreServicio, nameof(nombreServicio), 50);
        Guard.AgainstNullOrWhiteSpace(tipoServicio, nameof(tipoServicio), 50);

        NombreServicio = nombreServicio;
        TipoServicio = tipoServicio;
        AplicaTodasTemporadas = false;
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

    // Marca el servicio como disponible en todas las temporadas.
    // Ya no es necesario asignarlo temporada por temporada.
    public void HabilitarParaTodasTemporadas()
        => AplicaTodasTemporadas = true;

    // Revierte a modo por temporada — deberás asignarlo manualmente a cada una.
    public void DeshabilitarParaTodasTemporadas()
        => AplicaTodasTemporadas = false;

    public bool EstaDisponibleEn(int? temporadaId)
    {
        // Si aplica a todas, siempre está disponible sin importar la temporada
        if (AplicaTodasTemporadas) return true;

        // Sin temporada activa → disponible
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
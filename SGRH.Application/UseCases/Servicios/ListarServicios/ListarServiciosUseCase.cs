using SGRH.Application.Dtos.Servicios;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Servicios.ListarServicios;

public sealed class ListarServiciosUseCase
{
    private readonly IServicioAdicionalRepository _servicios;
    private readonly ITemporadaRepository _temporadas;

    public ListarServiciosUseCase(
        IServicioAdicionalRepository servicios,
        ITemporadaRepository temporadas)
    {
        _servicios = servicios;
        _temporadas = temporadas;
    }

    /// <summary>
    /// Lista servicios con filtro opcional por nombre y por temporada.
    ///
    /// Si se proporciona fechaEntrada:
    ///   1. Determina la temporada activa para esa fecha.
    ///   2. Si hay temporada activa → devuelve solo los servicios disponibles
    ///      en esa temporada (AplicaTodasTemporadas = true OR asignados a ella).
    ///   3. Si no hay temporada activa → devuelve todos los servicios
    ///      (no hay restricción estacional).
    ///
    /// Esto garantiza que el cliente solo vea servicios que puede agregar
    /// a su reserva sin que el dominio los rechace por temporada incorrecta.
    /// </summary>
    public async Task<ListarServiciosResponse> ExecuteAsync(
        string? nombre = null,
        DateTime? fechaEntrada = null,
        CancellationToken ct = default)
    {
        // Determinar temporada activa para filtrar servicios
        int? temporadaId = null;
        if (fechaEntrada.HasValue)
        {
            var temporada = await _temporadas.GetByFechaAsync(fechaEntrada.Value, ct);
            temporadaId = temporada?.TemporadaId;
        }

        var servicios = await _servicios.BuscarDisponiblesAsync(nombre, temporadaId, ct);

        var dtos = servicios.Select(s => s.ToDto()).ToList();

        return new ListarServiciosResponse(dtos);
    }
}
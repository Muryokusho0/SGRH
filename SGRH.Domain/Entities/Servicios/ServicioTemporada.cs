using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Servicios;

/// <summary>
/// Tabla junction que relaciona un ServicioAdicional con una Temporada.
/// Indica que el servicio está disponible para reservas dentro de esa temporada.
/// </summary>
public sealed class ServicioTemporada
{
    public int ServicioAdicionalId { get; private set; }
    public int TemporadaId { get; private set; }

    private ServicioTemporada() { }

    public ServicioTemporada(int servicioAdicionalId, int temporadaId)
    {
        Guard.AgainstOutOfRange(servicioAdicionalId, nameof(servicioAdicionalId), 0);
        Guard.AgainstOutOfRange(temporadaId, nameof(temporadaId), 0);

        ServicioAdicionalId = servicioAdicionalId;
        TemporadaId = temporadaId;
    }
}
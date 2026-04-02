using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Servicios;

/// <summary>
/// Tabla de unión (<em>junction table</em>) que relaciona un <c>ServicioAdicional</c> con una <c>Temporada</c>.
/// Indica que el servicio está disponible para ser contratado en reservas que caigan dentro de esa temporada.
/// </summary>
public sealed class ServicioTemporada
{
    /// <summary>
    /// Identificador del servicio adicional habilitado para la temporada.
    /// </summary>
    public int ServicioAdicionalId { get; private set; }

    /// <summary>
    /// Identificador de la temporada en la que el servicio está disponible.
    /// </summary>
    public int TemporadaId { get; private set; }

    private ServicioTemporada() { }

    /// <summary>
    /// Crea una nueva asociación entre un servicio adicional y una temporada.
    /// </summary>
    /// <param name="servicioAdicionalId">Id del servicio adicional (mayor a 0).</param>
    /// <param name="temporadaId">Id de la temporada (mayor a 0).</param>
    public ServicioTemporada(int servicioAdicionalId, int temporadaId)
    {
        Guard.AgainstOutOfRange(servicioAdicionalId, nameof(servicioAdicionalId), 0);
        Guard.AgainstOutOfRange(temporadaId, nameof(temporadaId), 0);

        ServicioAdicionalId = servicioAdicionalId;
        TemporadaId = temporadaId;
    }
}
using SGRH.Web.Models.Servicios;

namespace SGRH.Web.Services.Servicios;

public interface IServicioService
{
    /// <summary>
    /// Lista servicios disponibles para una fecha de entrada específica.
    /// Si se proporciona fechaEntrada, la API filtra por la temporada activa en esa fecha,
    /// devolviendo solo servicios que el dominio permitirá agregar a la reserva.
    /// Si fechaEntrada es null, devuelve todos los servicios sin filtro.
    /// </summary>
    Task<List<ServicioViewModel>> ListarAsync(
        DateTime? fechaEntrada = null,
        CancellationToken ct = default);
}
using SGRH.Web.Models.Temporadas;

namespace SGRH.Web.Services.Temporadas;

public interface ITemporadaService
{
    /// <summary>
    /// Lista todas las temporadas. El cliente solo ve nombre y fechas.
    /// </summary>
    Task<List<TemporadaViewModel>> ListarAsync(CancellationToken ct = default);
}
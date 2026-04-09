using SGRH.Web.Models.Categorias;

namespace SGRH.Web.Services.Categorias;

/// <summary>
/// Contrato del servicio de categorías de habitación para el portal del cliente.
/// </summary>
public interface ICategoriaService
{
    /// <summary>
    /// Lista las categorías de habitación para poblar el dropdown de filtro.
    /// El cliente selecciona de la lista, nunca ingresa el ID manualmente.
    /// </summary>
    Task<List<CategoriaViewModel>> ListarAsync(CancellationToken ct = default);
}
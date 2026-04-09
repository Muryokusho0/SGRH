using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Servicios;

/// <summary>
/// Representa un servicio adicional que el hotel ofrece a sus huéspedes
/// (por ejemplo: desayuno, spa, traslado al aeropuerto).
/// Controla su disponibilidad por temporada o para todas las temporadas.
/// </summary>
public sealed class ServicioAdicional : EntityBase
{
    /// <summary>
    /// Identificador único del servicio adicional.
    /// </summary>
    public int ServicioAdicionalId { get; private set; }

    /// <summary>
    /// Nombre descriptivo del servicio (por ejemplo: "Desayuno Buffet", "Masaje Relajante").
    /// </summary>
    public string NombreServicio { get; private set; } = default!;

    /// <summary>
    /// Tipo o categoría del servicio (por ejemplo: "Alimentación", "Bienestar", "Transporte").
    /// </summary>
    public string TipoServicio { get; private set; } = default!;

    /// <summary>
    /// Indica si el servicio está disponible en todas las temporadas sin necesidad
    /// de asignación explícita por temporada. Si es <c>true</c>, <see cref="TemporadaIds"/> se ignora.
    /// </summary>
    public bool AplicaTodasTemporadas { get; private set; }

    private readonly List<int> _temporadaIds = [];

    /// <summary>
    /// Colección de identificadores de temporadas en las que este servicio está explícitamente habilitado.
    /// Solo es relevante cuando <see cref="AplicaTodasTemporadas"/> es <c>false</c>.
    /// </summary>
    public IReadOnlyCollection<int> TemporadaIds => _temporadaIds;

    private ServicioAdicional() { }

    /// <summary>
    /// Crea un nuevo servicio adicional con nombre y tipo especificados.
    /// Por defecto no aplica a todas las temporadas.
    /// </summary>
    /// <param name="nombreServicio">Nombre del servicio (máx. 50 caracteres).</param>
    /// <param name="tipoServicio">Tipo o categoría del servicio (máx. 50 caracteres).</param>
    public ServicioAdicional(string nombreServicio, string tipoServicio)
    {
        Guard.AgainstNullOrWhiteSpace(nombreServicio, nameof(nombreServicio), 50);
        Guard.AgainstNullOrWhiteSpace(tipoServicio, nameof(tipoServicio), 50);

        NombreServicio = nombreServicio;
        TipoServicio = tipoServicio;
        AplicaTodasTemporadas = false;
    }

    /// <summary>
    /// Habilita el servicio para una temporada específica.
    /// </summary>
    /// <param name="temporadaId">Id de la temporada donde se habilitará el servicio (mayor a 0).</param>
    /// <exception cref="Exceptions.ConflictException">Si el servicio ya está habilitado para esa temporada.</exception>
    public void HabilitarEnTemporada(int temporadaId)
    {
        Guard.AgainstOutOfRange(temporadaId, nameof(temporadaId), 0);

        if (_temporadaIds.Contains(temporadaId))
            throw new ConflictException(
                $"El servicio ya está habilitado para la temporada {temporadaId}.");

        _temporadaIds.Add(temporadaId);
    }

    /// <summary>
    /// Deshabilita el servicio para una temporada específica.
    /// </summary>
    /// <param name="temporadaId">Id de la temporada de la que se quitará el servicio.</param>
    /// <exception cref="Exceptions.NotFoundException">Si el servicio no estaba habilitado en esa temporada.</exception>
    public void DeshabilitarEnTemporada(int temporadaId)
    {
        if (!_temporadaIds.Contains(temporadaId))
            throw new NotFoundException("ServicioTemporada", temporadaId.ToString());

        _temporadaIds.Remove(temporadaId);
    }

    /// <summary>
    /// Marca el servicio como disponible en todas las temporadas,
    /// eliminando la necesidad de asignación explícita por temporada.
    /// </summary>
    public void HabilitarParaTodasTemporadas()
        => AplicaTodasTemporadas = true;

    /// <summary>
    /// Revierte el servicio al modo de disponibilidad por temporada.
    /// Deberá asignarse manualmente a cada temporada deseada.
    /// </summary>
    public void DeshabilitarParaTodasTemporadas()
        => AplicaTodasTemporadas = false;

    /// <summary>
    /// Determina si el servicio está disponible para una temporada dada.
    /// </summary>
    /// <param name="temporadaId">Id de la temporada a verificar. Puede ser <c>null</c> si no hay temporada activa.</param>
    /// <returns>
    /// <c>true</c> si el servicio aplica a todas las temporadas, si no hay temporada activa,
    /// o si la temporada indicada está en la lista de habilitadas; de lo contrario, <c>false</c>.
    /// </returns>
    public bool EstaDisponibleEn(int? temporadaId)
    {
        // Servicio universal: disponible siempre
        if (AplicaTodasTemporadas) return true;

        // Servicio específico de temporada:
        // - Sin temporada activa → NO disponible
        //   (requiere que haya una temporada que lo incluya explícitamente)
        // - Con temporada activa → disponible solo si está asignado a esa temporada
        if (temporadaId is null) return false;

        return _temporadaIds.Contains(temporadaId.Value);
    }

    /// <summary>
    /// Actualiza el nombre y tipo del servicio adicional.
    /// </summary>
    /// <param name="nombreServicio">Nuevo nombre del servicio (máx. 50 caracteres).</param>
    /// <param name="tipoServicio">Nuevo tipo del servicio (máx. 50 caracteres).</param>
    public void Actualizar(string nombreServicio, string tipoServicio)
    {
        Guard.AgainstNullOrWhiteSpace(nombreServicio, nameof(nombreServicio), 50);
        Guard.AgainstNullOrWhiteSpace(tipoServicio, nameof(tipoServicio), 50);

        NombreServicio = nombreServicio;
        TipoServicio = tipoServicio;
    }

    protected override object GetKey() => ServicioAdicionalId;
}
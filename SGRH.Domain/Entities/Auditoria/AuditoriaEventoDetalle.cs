using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Auditoria;

/// <summary>
/// Representa el detalle de un cambio específico dentro de un evento de auditoría.
/// Registra el nombre del campo modificado, su valor anterior y su valor nuevo.
/// Es creado únicamente por <see cref="AuditoriaEvento"/> mediante el método
/// <c>AgregarDetalle</c>.
/// </summary>
public sealed class AuditoriaEventoDetalle : EntityBase
{
    /// <summary>
    /// Identificador único del detalle de auditoría.
    /// </summary>
    public long AuditoriaEventoDetalleId { get; private set; }

    /// <summary>
    /// Identificador del evento de auditoría al que pertenece este detalle.
    /// </summary>
    public long AuditoriaEventoId { get; private set; }

    /// <summary>
    /// Nombre del campo de la entidad que fue modificado.
    /// </summary>
    public string Campo { get; private set; } = default!;

    /// <summary>
    /// Valor del campo antes del cambio. Puede ser <c>null</c> si el campo no tenía valor previo.
    /// </summary>
    public string? ValorAnterior { get; private set; }

    /// <summary>
    /// Valor del campo después del cambio. Puede ser <c>null</c> si el campo fue eliminado.
    /// </summary>
    public string? ValorNuevo { get; private set; }

    private AuditoriaEventoDetalle() { }

    /// <summary>
    /// Crea un nuevo detalle de auditoría asociado a un evento.
    /// Solo puede ser instanciado desde el ensamblado del dominio.
    /// </summary>
    /// <param name="auditoriaEventoId">Id del evento de auditoría padre.</param>
    /// <param name="campo">Nombre del campo modificado.</param>
    /// <param name="valorAnterior">Valor anterior del campo.</param>
    /// <param name="valorNuevo">Valor nuevo del campo.</param>
    internal AuditoriaEventoDetalle(
        long auditoriaEventoId,
        string campo,
        string? valorAnterior,
        string? valorNuevo)
    {
        Guard.AgainstNullOrWhiteSpace(campo, nameof(campo), 128);

        AuditoriaEventoId = auditoriaEventoId;
        Campo = campo;
        ValorAnterior = valorAnterior;
        ValorNuevo = valorNuevo;
    }

    protected override object GetKey() => AuditoriaEventoDetalleId;
}
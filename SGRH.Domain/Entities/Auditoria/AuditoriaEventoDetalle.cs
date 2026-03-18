using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Auditoria;

public sealed class AuditoriaEventoDetalle : EntityBase
{
    public long AuditoriaEventoDetalleId { get; private set; }
    public long AuditoriaEventoId { get; private set; }
    public string Campo { get; private set; } = default!;
    public string? ValorAnterior { get; private set; }
    public string? ValorNuevo { get; private set; }

    private AuditoriaEventoDetalle() { }

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
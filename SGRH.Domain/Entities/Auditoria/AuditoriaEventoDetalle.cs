using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Auditoria;

public sealed class AuditoriaEventoDetalle : EntityBase
{
    public long AuditoriaEventoDetalleId { get; private set; }
    public long AuditoriaEventoId { get; private set; }

    public string Campo { get; private set; } = default!;      // nvarchar 128
    public string? ValorAnterior { get; private set; }         // nvarchar(max)
    public string? ValorNuevo { get; private set; }            // nvarchar(max)

    private AuditoriaEventoDetalle() { }

    public AuditoriaEventoDetalle(long auditoriaEventoId, string campo, string? valorAnterior, string? valorNuevo)
    {
        Guard.AgainstOutOfRange(auditoriaEventoId, nameof(auditoriaEventoId), 0);
        Guard.AgainstNullOrWhiteSpace(campo, nameof(campo), 128);

        AuditoriaEventoId = auditoriaEventoId;
        Campo = campo;
        ValorAnterior = valorAnterior;
        ValorNuevo = valorNuevo;
    }

    protected override object GetKey() => AuditoriaEventoDetalleId == 0
        ? $"{AuditoriaEventoId}-{Campo}"
        : AuditoriaEventoDetalleId;
}
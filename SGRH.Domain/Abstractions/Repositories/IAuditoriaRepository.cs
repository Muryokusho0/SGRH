using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Auditoria;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IAuditoriaRepository : IRepository<AuditoriaEvento, long>
{
    Task<List<AuditoriaEvento>> GetByUsuarioAsync(
        int usuarioId, CancellationToken ct = default);

    Task<List<AuditoriaEvento>> GetByModuloAsync(
        string modulo, CancellationToken ct = default);

    Task<List<AuditoriaEvento>> GetByRangoFechaAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default);

    Task<List<AuditoriaEvento>> BuscarAsync(
        string? modulo,
        string? accion,
        string? entidad,
        int? usuarioId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken ct = default);
}
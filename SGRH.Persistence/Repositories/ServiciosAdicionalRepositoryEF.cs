using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Servicios;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class ServicioAdicionalRepositoryEF
    : Repository<ServicioAdicional, int>, IServicioAdicionalRepository
{
    public ServicioAdicionalRepositoryEF(
        SGRHDbContext db,
        ILogger<ServicioAdicionalRepositoryEF> logger)
        : base(db, logger) { }

    public Task<bool> ExistsByNombreAsync(
        string nombreServicio, CancellationToken ct = default)
        => Db.ServiciosAdicionales
            .AnyAsync(s => s.NombreServicio == nombreServicio, ct);

    public async Task<ServicioAdicional?> GetByIdWithTemporadasAsync(
        int id, CancellationToken ct = default)
    {
        var servicio = await Db.ServiciosAdicionales
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServicioAdicionalId == id, ct);

        if (servicio is null) return null;

        var temporadaIds = await Db.ServicioTemporadas
            .AsNoTracking()
            .Where(st => st.ServicioAdicionalId == id)
            .Select(st => st.TemporadaId)
            .ToListAsync(ct);

        foreach (var tId in temporadaIds)
            servicio.HabilitarEnTemporada(tId);

        return servicio;
    }

    public Task<List<ServicioAdicional>> BuscarAsync(
        string? nombre, CancellationToken ct = default)
    {
        var query = Db.ServiciosAdicionales.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(s => s.NombreServicio.Contains(nombre));
        return query.OrderBy(s => s.NombreServicio).ToListAsync(ct);
    }

    /// <inheritdoc />
    public Task<List<ServicioAdicional>> BuscarDisponiblesAsync(
        string? nombre, int? temporadaId, CancellationToken ct = default)
    {
        var query = Db.ServiciosAdicionales.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(s => s.NombreServicio.Contains(nombre));

        if (temporadaId.HasValue)
        {
            var tId = temporadaId.Value;
            // Con temporada activa: servicios universales + los de esta temporada
            query = query.Where(s =>
                s.AplicaTodasTemporadas ||
                Db.ServicioTemporadas.Any(st =>
                    st.ServicioAdicionalId == s.ServicioAdicionalId &&
                    st.TemporadaId == tId));
        }
        else
        {
            // Sin temporada activa: SOLO servicios universales.
            // Los servicios específicos de temporada NO están disponibles
            // cuando no hay ninguna temporada activa — coherente con la
            // regla de negocio en ReservaDomainPolicy y ServicioAdicional.EstaDisponibleEn.
            query = query.Where(s => s.AplicaTodasTemporadas);
        }

        return query.OrderBy(s => s.NombreServicio).ToListAsync(ct);
    }
}
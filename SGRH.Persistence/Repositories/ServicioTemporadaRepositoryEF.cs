using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Persistence.Context;

// Alias explícito para evitar ambigüedad con SGRH.Persistence.Context.ServicioTemporada
// que aún puede existir. Eliminar SGRH.Persistence/Context/ServicioTemporada.cs
// para que este alias ya no sea necesario.
using ServicioTemporada = SGRH.Domain.Entities.Servicios.ServicioTemporada;

namespace SGRH.Persistence.Repositories;

public sealed class ServicioTemporadaRepositoryEF : IServicioTemporadaRepository
{
    private readonly SGRHDbContext _db;

    public ServicioTemporadaRepositoryEF(SGRHDbContext db) => _db = db;

    public Task<bool> ExisteAsync(
        int servicioId, int temporadaId, CancellationToken ct = default)
        => _db.ServicioTemporadas
            .AnyAsync(st => st.ServicioAdicionalId == servicioId
                         && st.TemporadaId == temporadaId, ct);

    public async Task AddAsync(ServicioTemporada entity, CancellationToken ct = default)
        => await _db.ServicioTemporadas.AddAsync(entity, ct);
}
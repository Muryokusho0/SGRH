using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Servicios;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class ServicioAdicionalRepositoryEF
    : Repository<ServicioAdicional, int>, IServicioAdicionalRepository
{
    public ServicioAdicionalRepositoryEF(SGRHDbContext db) : base(db) { }

    // Carga el servicio y puebla manualmente _temporadaIds desde ServicioTemporadas
    // (ServicioAdicional no tiene nav props — se carga mediante backing field)
    public async Task<ServicioAdicional?> GetByIdWithTemporadasAsync(
        int id, CancellationToken ct = default)
    {
        var servicio = await Db.ServiciosAdicionales
            .FirstOrDefaultAsync(s => s.ServicioAdicionalId == id, ct);

        if (servicio is null) return null;

        // Poblar _temporadaIds cargando los IDs desde la tabla junction
        var temporadaIds = await Db.ServicioTemporadas
            .AsNoTracking()
            .Where(st => st.ServicioAdicionalId == id)
            .Select(st => st.TemporadaId)
            .ToListAsync(ct);

        foreach (var tId in temporadaIds)
            servicio.HabilitarEnTemporada(tId);

        return servicio;
    }
}
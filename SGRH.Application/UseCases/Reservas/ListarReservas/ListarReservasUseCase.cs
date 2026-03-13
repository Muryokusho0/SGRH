using SGRH.Application.Dtos.Reservas;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Enums;

namespace SGRH.Application.UseCases.Reservas.ListarReservas;

public sealed class ListarReservasUseCase
{
    private readonly IReservaRepository _reservas;
    private readonly IClienteRepository _clientes;
    private readonly IHabitacionRepository _habitaciones;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly IServicioAdicionalRepository _servicios;

    public ListarReservasUseCase(
        IReservaRepository reservas,
        IClienteRepository clientes,
        IHabitacionRepository habitaciones,
        ICategoriaHabitacionRepository categorias,
        IServicioAdicionalRepository servicios)
    {
        _reservas = reservas;
        _clientes = clientes;
        _habitaciones = habitaciones;
        _categorias = categorias;
        _servicios = servicios;
    }

    public async Task<ListarReservasResponse> ExecuteAsync(
        int? clienteId = null,
        EstadoReserva? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        DateTime? reservadaDesde = null,
        DateTime? reservadaHasta = null,
        CancellationToken ct = default)
    {
        // Filtrado delegado al repositorio — sin in-memory filtering
        var reservas = await _reservas.BuscarAsync(
            clienteId,
            estado?.ToString(),
            fechaDesde,
            fechaHasta,
            reservadaDesde,
            reservadaHasta,
            ct);

        if (reservas.Count == 0)
            return new ListarReservasResponse([]);

        // ── Lookups en batch ──────────────────────────────────────────────
        var clienteIds = reservas.Select(r => r.ClienteId).Distinct().ToList();
        var habitacionIds = reservas.SelectMany(r => r.Habitaciones)
                                    .Select(h => h.HabitacionId).Distinct().ToList();
        var servicioIds = reservas.SelectMany(r => r.Servicios)
                                    .Select(s => s.ServicioAdicionalId).Distinct().ToList();

        var clientesMap = new Dictionary<int, string>();
        foreach (var cid in clienteIds)
        {
            var c = await _clientes.GetByIdAsync(cid, ct);
            if (c is not null)
                clientesMap[cid] = $"{c.NombreCliente} {c.ApellidoCliente}";
        }

        var habitacionesInfo = new Dictionary<int, (int NumeroHabitacion, string NombreCategoria)>();
        foreach (var hid in habitacionIds)
        {
            var hab = await _habitaciones.GetByIdAsync(hid, ct);
            if (hab is null) continue;
            var cat = await _categorias.GetByIdAsync(hab.CategoriaHabitacionId, ct);
            habitacionesInfo[hid] = (hab.NumeroHabitacion, cat?.NombreCategoria ?? string.Empty);
        }

        var serviciosInfo = new Dictionary<int, (string NombreServicio, string TipoServicio)>();
        foreach (var sid in servicioIds)
        {
            var srv = await _servicios.GetByIdAsync(sid, ct);
            if (srv is not null)
                serviciosInfo[sid] = (srv.NombreServicio, srv.TipoServicio);
        }

        var dtos = reservas
            .Select(r => r.ToDto(
                clientesMap.TryGetValue(r.ClienteId, out var n) ? n : string.Empty,
                habitacionesInfo,
                serviciosInfo))
            .ToList();

        return new ListarReservasResponse(dtos);
    }
}
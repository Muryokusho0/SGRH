using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.GetReserva;

public sealed class GetReservaUseCase
{
    private readonly IReservaRepository _reservas;
    private readonly IClienteRepository _clientes;
    private readonly IHabitacionRepository _habitaciones;
    private readonly ICategoriaHabitacionRepository _categorias;
    private readonly IServicioAdicionalRepository _servicios;

    public GetReservaUseCase(
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

    public async Task<GetReservaResponse> ExecuteAsync(
        int reservaId, CancellationToken ct = default)
    {
        var reserva = await _reservas.GetByIdWithDetallesAsync(reservaId, ct)
            ?? throw new NotFoundException("Reserva", reservaId.ToString());

        // ── Lookup cliente ────────────────────────────────────────────────
        var cliente = await _clientes.GetByIdAsync(reserva.ClienteId, ct)
            ?? throw new NotFoundException("Cliente", reserva.ClienteId.ToString());
        var nombreCliente = $"{cliente.NombreCliente} {cliente.ApellidoCliente}";

        // ── Lookup habitaciones + categorías ──────────────────────────────
        var habitacionesInfo = new Dictionary<int, (int NumeroHabitacion, string NombreCategoria)>();
        foreach (var detalle in reserva.Habitaciones)
        {
            if (habitacionesInfo.ContainsKey(detalle.HabitacionId)) continue;

            var hab = await _habitaciones.GetByIdAsync(detalle.HabitacionId, ct);
            if (hab is null) continue;

            var cat = await _categorias.GetByIdAsync(hab.CategoriaHabitacionId, ct);
            habitacionesInfo[detalle.HabitacionId] =
                (hab.NumeroHabitacion, cat?.NombreCategoria ?? string.Empty);
        }

        // ── Lookup servicios ──────────────────────────────────────────────
        var serviciosInfo = new Dictionary<int, (string NombreServicio, string TipoServicio)>();
        foreach (var rsa in reserva.Servicios)
        {
            if (serviciosInfo.ContainsKey(rsa.ServicioAdicionalId)) continue;

            var srv = await _servicios.GetByIdAsync(rsa.ServicioAdicionalId, ct);
            if (srv is null) continue;

            serviciosInfo[rsa.ServicioAdicionalId] =
                (srv.NombreServicio, srv.TipoServicio);
        }

        return new GetReservaResponse(
            reserva.ToDto(nombreCliente, habitacionesInfo, serviciosInfo));
    }
}
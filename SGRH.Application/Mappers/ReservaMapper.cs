using SGRH.Application.Dtos.Reservas;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class ReservaMapper
{
    public static ReservaDto ToDto(
        this Reserva reserva,
        string nombreCliente,
        Dictionary<int, (int NumeroHabitacion, string NombreCategoria)> habitacionesInfo,
        Dictionary<int, (string NombreServicio, string TipoServicio)> serviciosInfo) =>
        new(
            ReservaId: reserva.ReservaId,
            ClienteId: reserva.ClienteId,
            NombreCliente: nombreCliente,
            EstadoReserva: reserva.EstadoReserva.ToString(),
            FechaReserva: reserva.FechaReserva,
            FechaEntrada: reserva.FechaEntrada,
            FechaSalida: reserva.FechaSalida,
            CostoTotal: reserva.CostoTotal,
            Habitaciones: reserva.Habitaciones
                               .Select(h => h.ToDetalleDto(habitacionesInfo))
                               .ToList(),
            Servicios: reserva.Servicios
                               .Select(s => s.ToServicioDto(serviciosInfo))
                               .ToList());

    private static DetalleReservaDto ToDetalleDto(
        this DetalleReserva detalle,
        Dictionary<int, (int NumeroHabitacion, string NombreCategoria)> habitacionesInfo)
    {
        habitacionesInfo.TryGetValue(
            detalle.HabitacionId,
            out var info);

        return new DetalleReservaDto(
            HabitacionId: detalle.HabitacionId,
            NumeroHabitacion: info.NumeroHabitacion,
            NombreCategoria: info.NombreCategoria ?? string.Empty,
            TarifaAplicada: detalle.TarifaAplicada);
    }

    private static ReservaServicioDto ToServicioDto(
        this ReservaServicioAdicional rsa,
        Dictionary<int, (string NombreServicio, string TipoServicio)> serviciosInfo)
    {
        serviciosInfo.TryGetValue(
            rsa.ServicioAdicionalId,
            out var info);

        return new ReservaServicioDto(
            ServicioAdicionalId: rsa.ServicioAdicionalId,
            NombreServicio: info.NombreServicio ?? string.Empty,
            TipoServicio: info.TipoServicio ?? string.Empty,
            Cantidad: rsa.Cantidad,
            PrecioUnitario: rsa.PrecioUnitarioAplicado,
            Subtotal: rsa.SubTotal);
    }
}
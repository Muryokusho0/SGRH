using SGRH.Domain.Entities.Auditoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services;

/// <summary>
/// Define el contrato del servicio de auditoría encargado de persistir
/// los eventos de auditoría generados por las acciones de los usuarios.
/// La implementación reside en la capa de aplicación o infraestructura.
/// </summary>
public interface IAuditoriaService
{
    /// <summary>
    /// Registra y persiste un evento de auditoría en el sistema.
    /// </summary>
    /// <param name="evento">Evento de auditoría a registrar, con todos sus detalles.</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    Task RegistrarAsync(AuditoriaEvento evento, CancellationToken ct = default);
}

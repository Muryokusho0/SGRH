using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Define el contrato para notificar al administrador del sistema sobre errores inesperados
/// o situaciones de advertencia que requieren su atención.
/// La implementación reside en la capa de infraestructura y utiliza <see cref="IEmailSender"/>.
/// </summary>
public interface IAdminNotifier
{
    /// <summary>
    /// Notifica al administrador sobre un error inesperado capturado en el sistema.
    /// </summary>
    /// <param name="exception">Excepción capturada que describe el error.</param>
    /// <param name="context">
    /// Contexto adicional del error: endpoint, usuario, request id, IP, etc. Puede ser <c>null</c>.
    /// </param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    Task NotifyErrorAsync(
        Exception exception,
        AdminNotificationContext? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// Notifica al administrador sobre un evento de advertencia que no interrumpe el flujo
    /// del sistema pero que requiere atención (por ejemplo, un upload a S3 que falló y se reintentará).
    /// </summary>
    /// <param name="mensaje">Descripción legible de la advertencia.</param>
    /// <param name="context">
    /// Contexto adicional de la advertencia: endpoint, usuario, request id, etc. Puede ser <c>null</c>.
    /// </param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    Task NotifyWarningAsync(
        string mensaje,
        AdminNotificationContext? context = null,
        CancellationToken ct = default);
}

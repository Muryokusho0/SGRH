using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;
public interface IAdminNotifier
{
    // Notifica un error inesperado del sistema.
    // exception  → la excepción capturada
    // context    → info adicional: endpoint, usuario, request id, etc.
    Task NotifyErrorAsync(
        Exception exception,
        AdminNotificationContext? context = null,
        CancellationToken ct = default);

    // Notifica un evento de advertencia que no rompe el flujo
    // pero que los admins deben saber. Ejemplo: un S3 upload que falló
    // pero se reintentará, o una BD que tardó más de lo esperado.
    Task NotifyWarningAsync(
        string mensaje,
        AdminNotificationContext? context = null,
        CancellationToken ct = default);
}

using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Representa un destinatario o remitente de un correo electrónico,
/// compuesto por una dirección de correo y un nombre opcional.
/// </summary>
public sealed class EmailRecipient
{
    /// <summary>
    /// Dirección de correo electrónico del destinatario o remitente.
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// Nombre legible del destinatario o remitente (opcional).
    /// Por ejemplo: "Juan Pérez" o "Soporte SGRH".
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Crea un nuevo destinatario/remitente de correo electrónico.
    /// </summary>
    /// <param name="address">Dirección de correo electrónico (máx. 254 caracteres).</param>
    /// <param name="name">Nombre descriptivo del destinatario/remitente (opcional).</param>
    public EmailRecipient(string address, string? name = null)
    {
        Guard.AgainstNullOrWhiteSpace(address, nameof(address), 254);
        Name = name;
        Address = address;
    }
}

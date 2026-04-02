using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Representa una plantilla de correo electrónico identificada por una clave,
/// con variables de sustitución dinámicas que el proveedor de correo reemplazará al enviar.
/// </summary>
public sealed class EmailTemplate
{
    /// <summary>
    /// Clave o identificador de la plantilla en el proveedor de correo externo.
    /// </summary>
    public string TemplateKey { get; }

    /// <summary>
    /// Diccionario de variables clave-valor que se sustituirán en la plantilla al enviar el correo.
    /// Puede estar vacío si la plantilla no requiere variables dinámicas.
    /// </summary>
    public IDictionary<string, string> Variables { get; }

    /// <summary>
    /// Crea una nueva plantilla de correo con la clave y variables indicadas.
    /// </summary>
    /// <param name="templateKey">Clave de la plantilla en el proveedor (máx. 100 caracteres).</param>
    /// <param name="variables">Variables de sustitución (opcional). Si es <c>null</c>, se usa un diccionario vacío.</param>
    public EmailTemplate(string templateKey, IDictionary<string, string>? variables = null)
    {
        Guard.AgainstNullOrWhiteSpace(templateKey, nameof(templateKey), 100);
        TemplateKey = templateKey;
        Variables = variables ?? new Dictionary<string, string>();
    }
}

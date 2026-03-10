using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

public sealed class EmailTemplate
{
    public string TemplateKey { get; }
    public IDictionary<string, string> Variables { get; }

    public EmailTemplate(string templateKey, IDictionary<string, string>? variables = null)
    {
        Guard.AgainstNullOrWhiteSpace(templateKey, nameof(templateKey), 100);
        TemplateKey = templateKey;
        Variables = variables ?? new Dictionary<string, string>();
    }
}

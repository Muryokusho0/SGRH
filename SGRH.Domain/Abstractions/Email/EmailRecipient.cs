using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SGRH.Domain.Abstractions.Email;

public sealed class EmailRecipient
{
    public string Address { get; }
    public string? Name { get; }

    public EmailRecipient(string address, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Email Address es requerido.", nameof(address));

        Address = address;
        Name = name;
    }
}

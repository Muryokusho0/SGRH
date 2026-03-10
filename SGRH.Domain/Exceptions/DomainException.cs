using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public string Code { get; }
    public string? UserHint { get; }

    protected DomainException(string code, string message, string? userHint = null)
        : base(message)
    {
        Code = string.IsNullOrWhiteSpace(code) ? "DOMAIN_ERROR" : code;
        UserHint = userHint;
    }

    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = string.IsNullOrWhiteSpace(code) ? "DOMAIN_ERROR" : code;
    }
}
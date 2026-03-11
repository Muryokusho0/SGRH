using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Infrastructure.EmailSES;

public sealed class SesOptions
{
    public const string Section = "AWS:SES";

    public string Region { get; init; } = default!;
    public string FromAddress { get; init; } = default!;
    public string FromName { get; init; } = default!;
    public string AdminEmail { get; init; } = default!;
}
using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Auth.Login;

public sealed record LoginRequest(
    string Email,
    string Password,
    AuditInfo AuditInfo);

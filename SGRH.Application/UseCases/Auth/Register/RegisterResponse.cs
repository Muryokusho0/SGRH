using SGRH.Application.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Auth.Register;

public sealed record RegisterResponse(TokenDto Token);
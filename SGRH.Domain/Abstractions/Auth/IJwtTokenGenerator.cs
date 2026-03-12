using SGRH.Domain.Entities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    TokenResult Generate(Usuario usuario);
}
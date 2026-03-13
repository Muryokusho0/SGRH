using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Auth;
public sealed record TokenDto(
    string Token,
    DateTime ExpiresAtUtc,
    int UsuarioId,
    string Username,
    string Rol);
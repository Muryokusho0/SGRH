using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Usuarios;

public sealed record UsuarioDto(
    int UsuarioId,
    int? ClienteId,
    string Username,
    string Rol,
    bool Activo,
    DateTime CreatedAtUtc
);


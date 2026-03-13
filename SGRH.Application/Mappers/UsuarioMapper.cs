using SGRH.Application.Dtos.Usuarios;
using SGRH.Domain.Entities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class UsuarioMapper
{
    public static UsuarioDto ToDto(this Usuario usuario) =>
        new(
            UsuarioId: usuario.UsuarioId,
            ClienteId: usuario.ClienteId,
            Username: usuario.Username,
            Rol: usuario.Rol.ToString(),
            Activo: usuario.Activo,
            CreatedAtUtc: usuario.CreatedAtUtc);
}


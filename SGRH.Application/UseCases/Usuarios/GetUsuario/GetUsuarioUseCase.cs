using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Application.Mappers;

namespace SGRH.Application.UseCases.Usuarios.GetUsuario;
// Sin Request ni Validator — solo recibe el ID a consultar.
public sealed class GetUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;

    public GetUsuarioUseCase(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<GetUsuarioResponse> ExecuteAsync(
        int usuarioId, CancellationToken ct = default)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("Usuario", usuarioId.ToString());

        return new GetUsuarioResponse(usuario.ToDto());
    }
}


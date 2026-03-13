using SGRH.Application.Dtos.Usuarios;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Usuarios.ListarUsuarios;

public sealed class ListarUsuariosUseCase
{
    private readonly IUsuarioRepository _usuarios;

    public ListarUsuariosUseCase(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<ListarUsuariosResponse> ExecuteAsync(
        string? rol = null,
        bool? activo = null,
        CancellationToken ct = default)
    {
        var usuarios = await _usuarios.BuscarAsync(rol, activo, ct);

        var dtos = usuarios.Select(u => u.ToDto()).ToList();

        return new ListarUsuariosResponse(dtos);
    }
}
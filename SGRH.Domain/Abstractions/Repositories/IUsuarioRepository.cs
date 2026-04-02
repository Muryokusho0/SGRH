using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para usuarios del sistema.
/// Permite buscar por nombre de usuario, verificar duplicados y listar con filtros.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario, int>
{
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario (username).
    /// </summary>
    /// <param name="username">Nombre de usuario a buscar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El usuario encontrado, o <c>null</c> si no existe.</returns>
    Task<Usuario?> GetByUsernameAsync(
        string username, CancellationToken ct = default);

    /// <summary>
    /// Verifica si ya existe un usuario con el nombre de usuario indicado.
    /// </summary>
    /// <param name="username">Nombre de usuario a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si ya existe un usuario con ese username; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExistsByUsernameAsync(
        string username, CancellationToken ct = default);

    /// <summary>
    /// Busca usuarios con filtros opcionales de rol y estado activo/inactivo.
    /// </summary>
    /// <param name="rol">Filtrar por rol (por ejemplo: "ADMIN", "RECEPCIONISTA", "CLIENTE") (opcional).</param>
    /// <param name="activo">Filtrar por estado activo (<c>true</c>) o inactivo (<c>false</c>). Si es <c>null</c>, devuelve todos.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usuarios que cumplen los filtros indicados.</returns>
    Task<List<Usuario>> BuscarAsync(
        string? rol, bool? activo, CancellationToken ct = default);
}


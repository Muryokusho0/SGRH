using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IUsuarioRepository : IRepository<Usuario, int>
{
    Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct = default);
}

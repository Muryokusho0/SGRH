using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Abstractions.Auth;

namespace SGRH.Auth.Hashing;
// Implementa IPasswordHasher usando BCrypt.
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    // WorkFactor 12 = balance entre seguridad y rendimiento (~300ms por hash).
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}

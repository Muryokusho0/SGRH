using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SGRH.Domain.Abstractions.Auth;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;

namespace SGRH.Auth.Jwt;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenResult Generate(Usuario usuario)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpireMinutes);

        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,        usuario.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Username),
            new(ClaimTypes.Role,                    usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Incluir ClienteId solo para rol CLIENTE — los controllers lo usan
        // para filtrar que el cliente solo vea sus propios datos
        if (usuario.Rol == RolUsuario.CLIENTE && usuario.ClienteId.HasValue)
            claimsList.Add(new Claim("clienteId", usuario.ClienteId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claimsList,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResult(
            Token: tokenString,
            ExpiresAtUtc: expiresAt,
            UsuarioId: usuario.UsuarioId,
            Username: usuario.Username,
            Rol: usuario.Rol.ToString());
    }
}
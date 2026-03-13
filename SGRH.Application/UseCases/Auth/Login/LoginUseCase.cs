using SGRH.Application.Common.Exceptions;
using SGRH.Application.Dtos.Auth;
using SGRH.Domain.Abstractions.Auth;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Application.Abstractions;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Auth.Login;

public sealed class LoginUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<LoginRequest> _validator;

    public LoginUseCase(
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        IJwtTokenGenerator tokenGenerator,
        IAuditoriaService auditoria,
        IValidator<LoginRequest> validator)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<LoginResponse> ExecuteAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        // ── 1. Validar ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Buscar usuario por email (username = email en BD) ──────────
        var usuario = await _usuarios.GetByUsernameAsync(request.Email, ct);

        // Mensaje genérico — no revelar si el email existe o no
        if (usuario is null || !_hasher.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedException();

        if (!usuario.Activo)
            throw new UnauthorizedException("La cuenta está desactivada.");

        // ── 3. Generar token ──────────────────────────────────────────────
        var tokenResult = _tokenGenerator.Generate(usuario);

        // ── 4. Auditoría ──────────────────────────────────────────────────
        var evento = new AuditoriaEvento(
            usuarioId: usuario.UsuarioId,
            rol: usuario.Rol.ToString(),
            usernameSnapshot: usuario.Username,
            accion: "LOGIN",
            modulo: "Auth",
            entidad: "Usuario",
            entidadId: usuario.UsuarioId.ToString(),
            requestId: request.AuditInfo.RequestId,
            ipOrigen: request.AuditInfo.IpOrigen,
            userAgent: request.AuditInfo.UserAgent,
            descripcion: $"Inicio de sesión exitoso para '{usuario.Username}'.");

        await _auditoria.RegistrarAsync(evento, ct);

        // ── 5. Respuesta ──────────────────────────────────────────────────
        return new LoginResponse(new TokenDto(
            Token: tokenResult.Token,
            ExpiresAtUtc: tokenResult.ExpiresAtUtc,
            UsuarioId: tokenResult.UsuarioId,
            Username: tokenResult.Username,
            Rol: tokenResult.Rol));
    }
}
using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Dtos.Auth;
using SGRH.Domain.Abstractions.Auth;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Clientes;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Auth.Register;

public sealed class RegisterUseCase
{
    private readonly IClienteRepository _clientes;
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RegisterRequest> _validator;

    public RegisterUseCase(
        IClienteRepository clientes,
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IUnitOfWork uow,
        IValidator<RegisterRequest> validator)
    {
        _clientes = clientes;
        _usuarios = usuarios;
        _hasher = hasher;
        _jwt = jwt;
        _uow = uow;
        _validator = validator;
    }

    public async Task<RegisterResponse> ExecuteAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        // ── Validación ────────────────────────────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── Unicidad ──────────────────────────────────────────────────────
        if (await _clientes.GetByEmailAsync(request.Email, ct) is not null)
            throw new ConflictException("Ya existe un cliente registrado con ese email.");

        if (await _clientes.GetByNationalIdAsync(request.NationalId, ct) is not null)
            throw new ConflictException("Ya existe un cliente registrado con ese NationalId.");

        if (await _usuarios.ExistsByUsernameAsync(request.Email, ct))
            throw new ConflictException("Ya existe un usuario con ese email.");

        // ── Transacción: Cliente + Usuario ────────────────────────────────
        await _uow.BeginTransactionAsync(ct);
        try
        {
            // 1. Crear Cliente
            var cliente = new Cliente(
                request.NationalId,
                request.NombreCliente,
                request.ApellidoCliente,
                request.Email,
                request.Telefono);

            await _clientes.AddAsync(cliente, ct);
            await _uow.SaveChangesAsync(ct); // necesario para obtener ClienteId generado

            // 2. Crear Usuario con rol CLIENTE vinculado al Cliente
            var hash = _hasher.Hash(request.Password);
            var usuario = new Usuario(
                clienteId: cliente.ClienteId,
                username: request.Email,
                passwordHash: hash,
                rol: RolUsuario.CLIENTE);

            await _usuarios.AddAsync(usuario, ct);
            await _uow.SaveChangesAsync(ct);

            await _uow.CommitAsync(ct);

            // ── Generar token ─────────────────────────────────────────────
            var tokenResult = _jwt.Generate(usuario);

            return new RegisterResponse(new TokenDto(
                Token: tokenResult.Token,
                ExpiresAtUtc: tokenResult.ExpiresAtUtc,
                UsuarioId: tokenResult.UsuarioId,
                Username: tokenResult.Username,
                Rol: tokenResult.Rol));
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
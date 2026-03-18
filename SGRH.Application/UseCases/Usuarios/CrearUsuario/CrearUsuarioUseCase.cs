using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Auth;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Usuarios.CrearUsuario;

public sealed class CrearUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoriaService _auditoria;
    private readonly IValidator<CrearUsuarioRequest> _validator;

    public CrearUsuarioUseCase(
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork,
        IPasswordHasher hasher,
        IAuditoriaService auditoria,
        IValidator<CrearUsuarioRequest> validator)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
        _hasher = hasher;
        _auditoria = auditoria;
        _validator = validator;
    }

    public async Task<CrearUsuarioResponse> ExecuteAsync(
        CrearUsuarioRequest request,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        CancellationToken ct = default)
    {
        // ── 1. Validar — fuera de transacción ─────────────────────────────
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ApplicationValidationException(validation.Errors);

        // ── 2. Unicidad — lectura fuera de transacción ────────────────────
        if (await _usuarios.ExistsByUsernameAsync(request.Email, ct))
            throw new ConflictException(
                $"Ya existe un usuario con el email '{request.Email}'.");

        // El Validator garantiza que el valor es ADMIN o RECEPCIONISTA
        var rol = Enum.Parse<RolUsuario>(request.Rol, ignoreCase: true);

        // ── 3. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var usuario = new Usuario(
                clienteId: null,           // ADMIN/RECEPCIONISTA nunca tienen ClienteId
                username: request.Email,  // Username = Email
                passwordHash: _hasher.Hash(request.Password),
                rol: rol);

            await _usuarios.AddAsync(usuario, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "CREATE",
                modulo: "Usuarios",
                entidad: "Usuario",
                entidadId: usuario.UsuarioId.ToString(),
                requestId: request.AuditInfo.RequestId,
                ipOrigen: request.AuditInfo.IpOrigen,
                userAgent: request.AuditInfo.UserAgent,
                descripcion: $"Usuario '{request.Email}' creado con rol {rol}."), ct);

            await _unitOfWork.CommitAsync(ct);
            return new CrearUsuarioResponse(usuario.ToDto());
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
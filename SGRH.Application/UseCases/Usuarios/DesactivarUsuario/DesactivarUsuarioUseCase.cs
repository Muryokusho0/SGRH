using SGRH.Application.Abstractions;
using SGRH.Application.Common.Exceptions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;

namespace SGRH.Application.UseCases.Usuarios.DesactivarUsuario;

public sealed class DesactivarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoria;

    public DesactivarUsuarioUseCase(
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork,
        IAuditoriaService auditoria)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(
        int usuarioIdADesactivar,
        int usuarioActualId,
        string usuarioActualRol,
        string usernameActual,
        AuditInfo auditInfo,
        CancellationToken ct = default)
    {
        // ── 1. Regla de negocio — sin DB, fuera de transacción ────────────
        if (usuarioIdADesactivar == usuarioActualId)
            throw new BusinessRuleViolationException(
                "No puedes desactivar tu propia cuenta.");

        // ── 2. Buscar — lectura fuera de transacción ──────────────────────
        var usuario = await _usuarios.GetByIdAsync(usuarioIdADesactivar, ct)
            ?? throw new NotFoundException("Usuario", usuarioIdADesactivar.ToString());

        // ── 3. Transacción ────────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // La entidad lanza BusinessRuleViolationException si ya está inactivo
            usuario.Desactivar();

            await _unitOfWork.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(new AuditoriaEvento(
                usuarioId: usuarioActualId,
                rol: usuarioActualRol,
                usernameSnapshot: usernameActual,
                accion: "DEACTIVATE",
                modulo: "Usuarios",
                entidad: "Usuario",
                entidadId: usuario.UsuarioId.ToString(),
                requestId: auditInfo.RequestId,
                ipOrigen: auditInfo.IpOrigen,
                userAgent: auditInfo.UserAgent,
                descripcion: $"Usuario '{usuario.Username}' desactivado."), ct);

            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
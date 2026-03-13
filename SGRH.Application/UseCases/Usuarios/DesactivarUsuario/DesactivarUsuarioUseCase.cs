using SGRH.Application.Abstractions;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Application.Common.Exceptions;

namespace SGRH.Application.UseCases.Usuarios.DesactivarUsuario;
// Sin Request ni Validator — solo recibe el ID a desactivar.
// La regla de negocio (no desactivarse a sí mismo) se aplica aquí.
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
        // ── 1. Regla: un admin no puede desactivarse a sí mismo ───────────
        if (usuarioIdADesactivar == usuarioActualId)
            throw new BusinessRuleViolationException(
                "No puedes desactivar tu propia cuenta.");

        // ── 2. Buscar usuario ─────────────────────────────────────────────
        var usuario = await _usuarios.GetByIdAsync(usuarioIdADesactivar, ct)
            ?? throw new NotFoundException("Usuario", usuarioIdADesactivar.ToString());

        // ── 3. Desactivar (la entidad valida que no esté ya inactivo) ─────
        usuario.Desactivar();

        await _unitOfWork.SaveChangesAsync(ct);

        // ── 4. Auditoría ──────────────────────────────────────────────────
        var evento = new AuditoriaEvento(
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
            descripcion: $"Usuario '{usuario.Username}' desactivado.");

        await _auditoria.RegistrarAsync(evento, ct);
    }
}
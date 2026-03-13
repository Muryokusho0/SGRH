using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Auth.Register;

// Username NO se pide al usuario — se asigna automáticamente igual al Email.
// El campo Username en BD almacena el email, que es el identificador de login.
// En pantalla se muestra NombreCliente + ApellidoCliente como nombre visible.
public sealed record RegisterRequest(
    // ── Datos del Cliente ─────────────────────────────────────────────────
    string NationalId,
    string NombreCliente,
    string ApellidoCliente,
    string Telefono,
    // ── Credenciales ──────────────────────────────────────────────────────
    string Email,
    string Password,
    string ConfirmarPassword,
    // ── Auditoría ─────────────────────────────────────────────────────────
    AuditInfo AuditInfo);
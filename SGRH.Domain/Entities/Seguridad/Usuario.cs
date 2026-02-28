using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Entities.Clientes;

namespace SGRH.Domain.Entities.Seguridad;

    public sealed class Usuario : AuditableEntity
    {
        public int UsuarioId { get; private set; }
        public int? ClienteId { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public RolUsuario Rol { get; private set; }
        public bool Activo { get; private set; }

        private Usuario() { }

        public Usuario(
            int? clienteId,
            string username,
            string passwordHash,
            RolUsuario rol)
        {
            Guard.AgainstNullOrWhiteSpace(username, nameof(username), 100);
            Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash), 255);

            if (rol == RolUsuario.CLIENTE && clienteId is null)
                throw new DomainException("CLIENTE requiere ClienteId.");

            if ((rol == RolUsuario.ADMIN || rol == RolUsuario.RECEPCIONISTA)
                && clienteId is not null)
                throw new DomainException("ADMIN o RECEPCIONISTA no deben tener ClienteId.");

            ClienteId = clienteId;
            Username = username;
            PasswordHash = passwordHash;
            Rol = rol;
            Activo = true;
            CreatedAtUtc = DateTime.UtcNow;
        }

        protected override object GetKey() => UsuarioId;
    }

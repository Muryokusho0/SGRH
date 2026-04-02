using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Enums
{
    /// <summary>
    /// Define los roles de acceso disponibles para los usuarios del sistema.
    /// El rol determina los permisos y las restricciones de cada cuenta de usuario.
    /// </summary>
    public enum RolUsuario
    {
        /// <summary>
        /// Administrador del sistema. Tiene acceso completo a todas las funciones,
        /// incluyendo gestión de usuarios, configuración de habitaciones, temporadas y reportes.
        /// No debe tener un <c>ClienteId</c> asociado.
        /// </summary>
        ADMIN,

        /// <summary>
        /// Personal de recepción del hotel. Puede gestionar reservas, habitaciones
        /// y consultar reportes operativos, pero no administra usuarios ni configuración global.
        /// No debe tener un <c>ClienteId</c> asociado.
        /// </summary>
        RECEPCIONISTA,

        /// <summary>
        /// Huésped o cliente del hotel. Puede crear y consultar sus propias reservas.
        /// Debe tener un <c>ClienteId</c> asociado a su perfil de cliente.
        /// </summary>
        CLIENTE
    }
}

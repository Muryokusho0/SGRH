using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Clientes;

/// <summary>
/// Representa un cliente del hotel con sus datos personales de contacto.
/// Es la entidad central para el perfil de los huéspedes del sistema.
/// </summary>
public sealed class Cliente : EntityBase
{
    /// <summary>
    /// Identificador único del cliente en la base de datos.
    /// </summary>
    public int ClienteId { get; private set; }

    /// <summary>
    /// Documento de identidad nacional del cliente (cédula, pasaporte, etc.).
    /// Es único por cliente y no puede cambiarse una vez registrado.
    /// </summary>
    public string NationalId { get; private set; } = default!;

    /// <summary>
    /// Nombre(s) de pila del cliente.
    /// </summary>
    public string NombreCliente { get; private set; } = default!;

    /// <summary>
    /// Apellido(s) del cliente.
    /// </summary>
    public string ApellidoCliente { get; private set; } = default!;

    /// <summary>
    /// Dirección de correo electrónico del cliente.
    /// </summary>
    public string Email { get; private set; } = default!;

    /// <summary>
    /// Número de teléfono de contacto del cliente.
    /// </summary>
    public string Telefono { get; private set; } = default!;

    private Cliente() { }

    /// <summary>
    /// Crea un nuevo cliente con sus datos personales completos.
    /// </summary>
    /// <param name="nationalId">Documento de identidad nacional (máx. 20 caracteres).</param>
    /// <param name="nombreCliente">Nombre(s) del cliente (máx. 100 caracteres).</param>
    /// <param name="apellidoCliente">Apellido(s) del cliente (máx. 100 caracteres).</param>
    /// <param name="email">Correo electrónico del cliente (máx. 100 caracteres).</param>
    /// <param name="telefono">Teléfono de contacto del cliente (máx. 20 caracteres).</param>
    public Cliente(
        string nationalId,
        string nombreCliente,
        string apellidoCliente,
        string email,
        string telefono)
    {
        Guard.AgainstNullOrWhiteSpace(nationalId, nameof(nationalId), 20);
        Guard.AgainstNullOrWhiteSpace(nombreCliente, nameof(nombreCliente), 100);
        Guard.AgainstNullOrWhiteSpace(apellidoCliente, nameof(apellidoCliente), 100);
        Guard.AgainstNullOrWhiteSpace(email, nameof(email), 100);
        Guard.AgainstNullOrWhiteSpace(telefono, nameof(telefono), 20);

        NationalId = nationalId;
        NombreCliente = nombreCliente;
        ApellidoCliente = apellidoCliente;
        Email = email;
        Telefono = telefono;
    }

    /// <summary>
    /// Actualiza los datos personales del cliente. El <c>NationalId</c> no puede modificarse.
    /// </summary>
    /// <param name="nombreCliente">Nuevo nombre(s) del cliente (máx. 100 caracteres).</param>
    /// <param name="apellidoCliente">Nuevo apellido(s) del cliente (máx. 100 caracteres).</param>
    /// <param name="email">Nuevo correo electrónico del cliente (máx. 100 caracteres).</param>
    /// <param name="telefono">Nuevo teléfono de contacto (máx. 20 caracteres).</param>
    public void ActualizarDatos(
        string nombreCliente,
        string apellidoCliente,
        string email,
        string telefono)
    {
        Guard.AgainstNullOrWhiteSpace(nombreCliente, nameof(nombreCliente), 100);
        Guard.AgainstNullOrWhiteSpace(apellidoCliente, nameof(apellidoCliente), 100);
        Guard.AgainstNullOrWhiteSpace(email, nameof(email), 100);
        Guard.AgainstNullOrWhiteSpace(telefono, nameof(telefono), 20);

        NombreCliente = nombreCliente;
        ApellidoCliente = apellidoCliente;
        Email = email;
        Telefono = telefono;
    }

    protected override object GetKey() => ClienteId;
}

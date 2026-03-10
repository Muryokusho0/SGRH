using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Clientes;

public sealed class Cliente : EntityBase
{
    public int ClienteId { get; private set; }
    public string NationalId { get; private set; } = default!;
    public string NombreCliente { get; private set; } = default!;
    public string ApellidoCliente { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Telefono { get; private set; } = default!;

    private Cliente() { }

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

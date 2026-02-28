using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Clientes;

public class Cliente : EntityBase
{
    public int ClienteId { get; private set; }
    public string NationalID { get; private set; } = default!;
    public string NombreCliente { get; private set; } = default!;
    public string ApellidoCliente { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Telefono { get; private set; } = default!;

    private Cliente() { }

    public Cliente(string nationalId, string nombre, string apellido, string email, string telefono)
    {
        Guard.AgainstNullOrWhiteSpace(nationalId, nameof(nationalId), 20);
        Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre), 100);
        Guard.AgainstNullOrWhiteSpace(apellido, nameof(apellido), 100);
        Guard.AgainstNullOrWhiteSpace(email, nameof(email), 100);
        Guard.AgainstNullOrWhiteSpace(telefono, nameof(telefono), 20);

        NationalID = nationalId;
        NombreCliente = nombre;
        ApellidoCliente = apellido;
        Email = email;
        Telefono = telefono;
    }

    protected override object GetKey() => ClienteId;
}

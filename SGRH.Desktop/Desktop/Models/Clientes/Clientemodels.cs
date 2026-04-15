namespace Desktop.Models.Clientes;

public sealed class ClienteResumen
{
    public int ClienteId { get; init; }
    public string NationalId { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
}

public sealed class CrearClienteRequest
{
    public string NationalId { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string ApellidoCliente { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
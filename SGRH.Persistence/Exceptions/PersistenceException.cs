namespace SGRH.Persistence.Exceptions;

/// <summary>
/// Excepción lanzada por la capa de persistencia cuando una operación de base
/// de datos falla de forma controlada (violación de constraint, concurrencia, etc.).
/// Envuelve la excepción original de EF Core para preservar la causa raíz,
/// pero expone un mensaje legible para el middleware de la API.
/// </summary>
public sealed class PersistenceException : Exception
{
    /// <summary>Tipo de error que ocurrió en la base de datos.</summary>
    public PersistenceErrorType ErrorType { get; }

    public PersistenceException(
        string message,
        PersistenceErrorType errorType,
        Exception? inner = null)
        : base(message, inner)
    {
        ErrorType = errorType;
    }
}

/// <summary>
/// Clasifica los tipos de error que pueden ocurrir al persistir datos.
/// Permite al middleware mapear cada tipo a un código HTTP apropiado.
/// </summary>
public enum PersistenceErrorType
{
    /// <summary>
    /// Violación de restricción única (UNIQUE INDEX / PRIMARY KEY duplicado).
    /// Mapea a 409 Conflict.
    /// </summary>
    UniqueConstraintViolation,

    /// <summary>
    /// Violación de clave foránea (FK inexistente o con dependencias).
    /// Mapea a 400 Bad Request o 422 Unprocessable Entity.
    /// </summary>
    ForeignKeyViolation,

    /// <summary>
    /// Conflicto de concurrencia optimista (registro modificado por otra transacción).
    /// Mapea a 409 Conflict.
    /// </summary>
    ConcurrencyConflict,

    /// <summary>
    /// Error general de base de datos no clasificado en los tipos anteriores.
    /// Mapea a 500 Internal Server Error.
    /// </summary>
    GeneralDatabaseError
}
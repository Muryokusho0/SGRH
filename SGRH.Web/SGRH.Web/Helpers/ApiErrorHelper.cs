using SGRH.Web.Models;

namespace SGRH.Web.Helpers;

/// <summary>
/// Traduce los errores de SGRH.Api a mensajes apropiados para el cliente.
///
/// Filosofía:
/// - Si el mensaje de la API ya es legible en español → mostrarlo directamente.
/// - Si el mensaje es técnico o contiene nombres de clases/IDs → traducirlo.
/// - Nunca exponer stack traces, nombres de excepción ni detalles internos.
///
/// El middleware de la API devuelve: { "status": int, "title": string, "errors": string[] }
///
/// Códigos:
///   400 → Validación (ApplicationValidationException)
///   401 → Credenciales inválidas (UnauthorizedException)
///   404 → No encontrado (NotFoundException)
///   409 → Conflicto/duplicado (ConflictException)
///   422 → Regla de negocio (BusinessRuleViolationException)
///   500 → Error interno
/// </summary>
public static class ApiErrorHelper
{
    // ── Punto de entrada principal ────────────────────────────────────────

    public static string TraducirError(ApiErrorResponse error)
        => error.Status switch
        {
            400 => TraducirValidacion(error.Errors),
            401 => TraducirAutenticacion(error.Title),
            403 => "No tienes permiso para realizar esta acción.",
            404 => TraducirNoEncontrado(error.Title),
            409 => TraducirConflicto(error.Title),
            422 => TraducirReglaNegocio(error.Title),
            500 => "Ocurrió un error inesperado en el servidor. Por favor intenta de nuevo.",
            _ => "Ocurrió un error inesperado. Por favor intenta de nuevo."
        };

    /// <summary>
    /// Traduce excepciones de red o comunicación HTTP.
    /// </summary>
    public static string TraducirExcepcion(Exception ex) => ex switch
    {
        HttpRequestException =>
            "No se pudo conectar con el servidor. Verifica tu conexión e intenta de nuevo.",
        TaskCanceledException or OperationCanceledException =>
            "La solicitud tardó demasiado. Verifica tu conexión e intenta de nuevo.",
        System.Text.Json.JsonException =>
            "La respuesta del servidor no pudo procesarse. Intenta de nuevo.",
        _ =>
            "Ocurrió un error inesperado. Por favor intenta de nuevo."
    };

    // ── 400 — Validación ──────────────────────────────────────────────────

    /// <summary>
    /// Los mensajes de validación del RegisterValidator y LoginValidator ya están
    /// escritos en español y son legibles para el cliente. Los mostramos directamente.
    /// Solo traducimos los que parezcan técnicos.
    /// </summary>
    private static string TraducirValidacion(IReadOnlyList<string> errores)
    {
        if (errores.Count == 0)
            return "Los datos ingresados no son válidos. Revisa los campos e intenta de nuevo.";

        var mensajes = errores
            .Select(e => EsTecnico(e) ? TraducirMensajeValidacion(e) : e)
            .Distinct()
            .ToList();

        return mensajes.Count == 1
            ? mensajes[0]
            : "Hay varios campos incorrectos:\n• " + string.Join("\n• ", mensajes);
    }

    // ── 401 — Autenticación ───────────────────────────────────────────────

    private static string TraducirAutenticacion(string? titulo)
    {
        var t = titulo?.ToLowerInvariant() ?? string.Empty;

        // Si el mensaje de la API ya es claro, mostrarlo
        if (titulo is not null && !EsTecnico(titulo) && titulo.Length <= 120)
            return titulo;

        if (t.Contains("credencial") || t.Contains("contraseña") || t.Contains("usuario"))
            return "El correo o la contraseña son incorrectos.";

        if (t.Contains("expirado") || t.Contains("token"))
            return "Tu sesión expiró. Por favor inicia sesión nuevamente.";

        return "Correo o contraseña incorrectos. Verifica tus datos e intenta de nuevo.";
    }

    // ── 404 — No encontrado ───────────────────────────────────────────────

    /// <summary>
    /// Convierte nombres técnicos de entidades a términos legibles.
    /// </summary>
    private static string TraducirNoEncontrado(string? titulo)
    {
        var t = titulo?.ToLowerInvariant() ?? string.Empty;

        if (t.Contains("reserva")) return "No encontramos esa reserva.";
        if (t.Contains("detallereserva")) return "No encontramos esa habitación en tu reserva.";
        if (t.Contains("reservaservicioadicional"))
            return "No encontramos ese servicio en tu reserva.";
        if (t.Contains("habitacion") || t.Contains("habitación"))
            return "No encontramos esa habitación en tu reserva.";
        if (t.Contains("servicio")) return "No encontramos ese servicio en tu reserva.";
        if (t.Contains("cliente")) return "No encontramos tu perfil de cliente.";
        if (t.Contains("temporada")) return "No hay una temporada activa configurada.";
        if (t.Contains("tarifa")) return "No hay tarifa disponible para esas fechas.";
        if (t.Contains("categoria") || t.Contains("categoría"))
            return "La categoría seleccionada no existe.";
        if (t.Contains("usuario")) return "El usuario no fue encontrado.";

        return "El elemento que buscas no existe o fue eliminado.";
    }

    // ── 409 — Conflicto ───────────────────────────────────────────────────

    /// <summary>
    /// Los mensajes de ConflictException del dominio y los UseCases ya son
    /// legibles en español. Si no coinciden con un patrón de reserva/habitación,
    /// se muestran directamente (ej: "Ya existe un cliente registrado con ese email.").
    /// </summary>
    private static string TraducirConflicto(string? titulo)
    {
        var t = titulo?.ToLowerInvariant() ?? string.Empty;

        // Conflictos específicos de reservas
        if (t.Contains("habitación ya está incluida") || t.Contains("habitacion ya está incluida"))
            return "Esa habitación ya está en tu reserva.";

        if (t.Contains("servicio ya está en la reserva"))
            return "Ese servicio ya está en tu reserva. Ajusta la cantidad si necesitas más.";

        // Para cualquier otro conflicto (auth, registro, etc.) el mensaje
        // de la API ya es legible — mostrarlo directamente.
        if (titulo is not null && !EsTecnico(titulo) && titulo.Length <= 150)
            return titulo;

        return "Ya existe un registro con esos datos.";
    }

    // ── 422 — Regla de negocio ────────────────────────────────────────────

    /// <summary>
    /// Mapea los mensajes exactos del dominio (Reserva.cs, Habitacion.cs).
    /// Si el mensaje ya es legible y no técnico, se muestra directamente.
    /// </summary>
    private static string TraducirReglaNegocio(string? titulo)
    {
        var t = titulo?.ToLowerInvariant() ?? string.Empty;

        // ── Estado de la reserva ─────────────────────────────────────────
        if (t.Contains("confirmada no puede ser modificada"))
            return "Esta reserva ya fue confirmada y no puede modificarse.";

        if (t.Contains("cancelada no puede ser modificada"))
            return "Esta reserva fue cancelada y no puede modificarse.";

        if (t.Contains("finalizada no puede ser modificada"))
            return "Esta reserva ya fue completada y no puede modificarse.";

        // ── Confirmación ─────────────────────────────────────────────────
        if (t.Contains("solo una reserva pendiente puede confirmarse"))
            return "Solo puedes confirmar una reserva que esté pendiente.";

        if (t.Contains("sin habitaciones") && t.Contains("confirmar"))
            return "Agrega al menos una habitación antes de confirmar tu reserva.";

        // ── Cancelación ──────────────────────────────────────────────────
        if (t.Contains("finalizada no puede cancelarse"))
            return "Esta reserva ya fue completada y no puede cancelarse.";

        if (t.Contains("ya está cancelada"))
            return "Esta reserva ya estaba cancelada.";

        // ── Fechas ───────────────────────────────────────────────────────
        if (t.Contains("anterior a") && (t.Contains("entrada") || t.Contains("salida")))
            return "La fecha de entrada debe ser anterior a la fecha de salida.";

        // ── Habitaciones ─────────────────────────────────────────────────
        if (t.Contains("habitacion") || t.Contains("habitación"))
        {
            if (t.Contains("disponible"))
                return "Una de las habitaciones no está disponible para las fechas seleccionadas.";
            if (t.Contains("mantenimiento"))
                return "Una de las habitaciones está en mantenimiento para esas fechas.";
            if (t.Contains("ya se encuentra en estado"))
                return "La habitación ya se encuentra en ese estado.";
        }

        // ── Servicios ────────────────────────────────────────────────────
        if (t.Contains("al menos una habitación antes de agregar servicios"))
            return "Primero agrega una habitación para poder incluir servicios.";

        if (t.Contains("servicio") && t.Contains("temporada"))
            return "Ese servicio no está disponible para las fechas de tu reserva.";

        // ── Para cualquier otro mensaje legible, mostrarlo directamente ──
        // Esto incluye mensajes del Guard y del dominio que ya están en español
        if (titulo is not null && !EsTecnico(titulo) && titulo.Length <= 150)
            return titulo;

        return "No se pudo completar la operación. Verifica los datos e intenta de nuevo.";
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Determina si un mensaje es técnico y debe traducirse,
    /// o si ya es legible y puede mostrarse directamente al cliente.
    /// </summary>
    private static bool EsTecnico(string mensaje)
        => mensaje.Contains("Exception")
        || mensaje.Contains("nameof")
        || mensaje.Contains("Id=")
        || mensaje.Contains("=>")
        || mensaje.Contains("null")
        || mensaje.Contains("System.")
        || mensaje.Length > 200;

    /// <summary>
    /// Fallback para mensajes de validación que parezcan técnicos.
    /// </summary>
    private static string TraducirMensajeValidacion(string mensaje)
    {
        var m = mensaje.ToLowerInvariant();

        if (m.Contains("fechaentrada") || m.Contains("fecha de entrada"))
            return "La fecha de entrada no es válida.";
        if (m.Contains("fechasalida") || m.Contains("fecha de salida"))
            return "La fecha de salida no es válida.";
        if (m.Contains("clienteid"))
            return "El cliente no es válido.";
        if (m.Contains("username") || m.Contains("usuario"))
            return "El nombre de usuario no puede estar vacío.";
        if (m.Contains("password") || m.Contains("contraseña"))
            return "La contraseña no es válida.";
        if (m.Contains("email") || m.Contains("correo"))
            return "El correo electrónico no es válido.";
        if (m.Contains("telefono") || m.Contains("teléfono"))
            return "El teléfono no es válido.";
        if (m.Contains("cantidad"))
            return "La cantidad debe ser mayor a cero.";
        if (m.Contains("requerido") || m.Contains("obligatorio") || m.Contains("required"))
            return "Hay campos obligatorios sin completar.";

        return "Uno de los datos ingresados no es válido.";
    }
}
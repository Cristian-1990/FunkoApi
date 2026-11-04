namespace FunkoAppi01.Errors;

/// <summary>
/// Errores de dominio. Cada tipo de error construye su propio mensaje.
/// </summary>
/// <param name="Message">Mensaje.</param>
public abstract record DomainError(string Message)
{
    /// <summary>Cuando no existe ningún registro con ese identificador.</summary>
    /// <param name="Id">Identificador buscado.</param>
    /// <param name="Entidad">Tipo de entidad buscada (por defecto, Funko).</param>
    public sealed record NotFound(long Id, string Entidad = "Funko")
        : DomainError($"No se ha encontrado ningún registro de {Entidad} con el identificador: {Id}");

    /// <summary>Los datos no cumplen con el validador.</summary>
    /// <param name="Errors">Lista de errores detectados.</param>
    public sealed record Validation(IEnumerable<string> Errors)
        : DomainError(string.Join(", ", Errors));

    /// <summary>Ya existe un registro activo con ese nombre.</summary>
    /// <param name="Nombre">Nombre duplicado.</param>
    /// <param name="Entidad">Tipo de entidad duplicada (por defecto, Funko).</param>
    public sealed record AlreadyExists(string Nombre, string Entidad = "Funko")
        : DomainError($"Ya existe un registro de {Entidad} con el nombre: {Nombre}");

    /// <summary>Credenciales inválidas o token no válido.</summary>
    /// <param name="Reason">Motivo.</param>
    public sealed record Unauthorized(string Reason) : DomainError(Reason);

    /// <summary>Conflicto genérico (p. ej. un email ya registrado).</summary>
    /// <param name="Reason">Motivo.</param>
    public sealed record Conflict(string Reason) : DomainError(Reason);
}

/// <summary>
/// Factoría para crear errores de dominio de Funko.
/// </summary>
public static class FunkoErrors
{
    /// <summary>Funko no encontrado.</summary>
    public static DomainError NotFound(long id) => new DomainError.NotFound(id);

    /// <summary>Validación con la lista de errores.</summary>
    public static DomainError Validation(IEnumerable<string> errors) => new DomainError.Validation(errors);

    /// <summary>Nombre ya existente entre los Funkos activos.</summary>
    public static DomainError AlreadyExists(string nombre) => new DomainError.AlreadyExists(nombre);
}

/// <summary>
/// Factoría para crear errores de dominio de autenticación.
/// </summary>
public static class AuthErrors
{
    /// <summary>Credenciales inválidas.</summary>
    public static DomainError Unauthorized(string reason) => new DomainError.Unauthorized(reason);

    /// <summary>Email ya registrado.</summary>
    public static DomainError Conflict(string reason) => new DomainError.Conflict(reason);
}

/// <summary>
/// Factoría para crear errores de dominio de Categoria.
/// </summary>
public static class CategoriaErrors
{
    /// <summary>Categoria no encontrada.</summary>
    public static DomainError NotFound(long id) => new DomainError.NotFound(id, "Categoria");

    /// <summary>Validación con la lista de errores.</summary>
    public static DomainError Validation(IEnumerable<string> errors) => new DomainError.Validation(errors);

    /// <summary>Nombre ya existente entre las categorías activas.</summary>
    public static DomainError AlreadyExists(string nombre) => new DomainError.AlreadyExists(nombre, "Categoria");
}

/// <summary>
/// Factoría para crear errores de dominio de Proveedor.
/// </summary>
public static class ProveedorErrors
{
    /// <summary>Proveedor no encontrado.</summary>
    public static DomainError NotFound(long id) => new DomainError.NotFound(id, "Proveedor");

    /// <summary>Validación con la lista de errores.</summary>
    public static DomainError Validation(IEnumerable<string> errors) => new DomainError.Validation(errors);

    /// <summary>Nombre ya existente entre los proveedores activos.</summary>
    public static DomainError AlreadyExists(string nombre) => new DomainError.AlreadyExists(nombre, "Proveedor");
}

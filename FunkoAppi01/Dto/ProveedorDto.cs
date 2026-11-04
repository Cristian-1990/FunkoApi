namespace FunkoAppi01.Dto;

/// <summary>
/// DTO con los datos de un Proveedor que el servicio devuelve hacia fuera.
/// </summary>
/// <param name="Id">Identificador.</param>
/// <param name="Nombre">Nombre del proveedor.</param>
/// <param name="Telefono">Teléfono de contacto.</param>
/// <param name="CreatedAt">Fecha de creación.</param>
/// <param name="UpdatedAt">Fecha de la última actualización.</param>
public record ProveedorDto(
    long Id,
    string Nombre,
    string Telefono,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

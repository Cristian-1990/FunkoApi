namespace FunkoAppi01.Dto;

/// <summary>
/// DTO con los datos de un Funko que el servicio devuelve hacia fuera.
/// </summary>
/// <param name="Id">Identificador.</param>
/// <param name="Nombre">Nombre del Funko.</param>
/// <param name="Precio">Precio de venta.</param>
/// <param name="Stock">Unidades disponibles.</param>
/// <param name="Categoria">Categoría a la que pertenece.</param>
/// <param name="Proveedor">Proveedor que lo suministra (opcional).</param>
/// <param name="CreatedAt">Fecha de creación.</param>
/// <param name="UpdatedAt">Fecha de la última actualización.</param>
public record FunkoDto(
    long Id,
    string Nombre,
    decimal Precio,
    int Stock,
    string Categoria,
    string? Proveedor,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

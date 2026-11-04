namespace FunkoAppi01.Dto;

/// <summary>
/// DTO con los datos de una Categoria que el servicio devuelve hacia fuera.
/// </summary>
/// <param name="Id">Identificador.</param>
/// <param name="Nombre">Nombre de la categoría.</param>
/// <param name="CreatedAt">Fecha de creación.</param>
/// <param name="UpdatedAt">Fecha de la última actualización.</param>
public record CategoriaDto(
    long Id,
    string Nombre,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

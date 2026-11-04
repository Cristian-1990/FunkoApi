using FunkoAppi01.Dto;
using FunkoAppi01.Models;

namespace FunkoAppi01.Mappers;

/// <summary>
/// Métodos de extensión para convertir entre los DTOs y el modelo de dominio de Categoria.
/// </summary>
public static class CategoriaMapper
{
    public static Categoria ToCategoria(this CreateCategoriaDto request) => new()
    {
        Nombre = request.Nombre
    };

    public static Categoria ToCategoria(this UpdateCategoriaDto request) => new()
    {
        Nombre = request.Nombre
    };

    public static CategoriaDto ToDto(this Categoria categoria) => new(
        categoria.Id,
        categoria.Nombre,
        categoria.CreatedAt,
        categoria.UpdatedAt);
}

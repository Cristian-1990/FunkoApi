using FunkoAppi01.Entity;
using FunkoAppi01.Models;

namespace FunkoAppi01.Mappers;

/// <summary>
/// Convierte entre la entidad de persistencia y el modelo de dominio de Categoria.
/// </summary>
public static class CategoriaEntityMapper
{
    public static Categoria ToCategoria(this CategoriaEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}

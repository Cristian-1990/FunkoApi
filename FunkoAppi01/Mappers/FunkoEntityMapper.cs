using FunkoAppi01.Entity;
using FunkoAppi01.Models;

namespace FunkoAppi01.Mappers;

/// <summary>
/// Convierte entre la entidad de persistencia y el modelo de dominio.
/// La categoría se aplana a su nombre: el modelo de dominio no conoce la relación,
/// solo la tabla. Resolver/crear la fila de categoría es responsabilidad del repositorio.
/// </summary>
public static class FunkoEntityMapper
{
    public static Funko ToFunko(this FunkoEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Precio = entity.Precio,
        Stock = entity.Stock,
        Categoria = entity.Categoria.Nombre,
        Proveedor = entity.Proveedor?.Nombre,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}

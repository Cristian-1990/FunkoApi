using FunkoAppi01.Entity;
using FunkoAppi01.Models;

namespace FunkoAppi01.Mappers;

/// <summary>
/// Convierte entre la entidad de persistencia y el modelo de dominio de Proveedor.
/// </summary>
public static class ProveedorEntityMapper
{
    public static Proveedor ToProveedor(this ProveedorEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Telefono = entity.Telefono,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}

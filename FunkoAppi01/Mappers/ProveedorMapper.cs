using FunkoAppi01.Dto;
using FunkoAppi01.Models;

namespace FunkoAppi01.Mappers;

/// <summary>
/// Métodos de extensión para convertir entre los DTOs y el modelo de dominio de Proveedor.
/// </summary>
public static class ProveedorMapper
{
    public static Proveedor ToProveedor(this CreateProveedorDto request) => new()
    {
        Nombre = request.Nombre,
        Telefono = request.Telefono
    };

    public static Proveedor ToProveedor(this UpdateProveedorDto request) => new()
    {
        Nombre = request.Nombre,
        Telefono = request.Telefono
    };

    public static ProveedorDto ToDto(this Proveedor proveedor) => new(
        proveedor.Id,
        proveedor.Nombre,
        proveedor.Telefono,
        proveedor.CreatedAt,
        proveedor.UpdatedAt);
}

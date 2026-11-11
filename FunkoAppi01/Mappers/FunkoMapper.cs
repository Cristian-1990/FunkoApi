using FunkoAppi01.Dto;
using FunkoAppi01.Models;

namespace FunkoAppi01.Mappers;

/// <summary>
/// Métodos de extensión para convertir entre los DTOs y el modelo de dominio.
/// Centraliza la conversión entre capas.
/// </summary>
public static class FunkoMapper
{
    /// <summary>Convierte una petición de creación en Funko. El Id lo asigna el repositorio.</summary>
    public static Funko ToFunko(this CreateFunkoDto request) => new()
    {
        Nombre = request.Nombre,
        Precio = request.Precio,
        Stock = request.Stock,
        Categoria = request.Categoria,
        Proveedor = request.Proveedor
    };

    /// <summary>Convierte una petición de actualización en Funko.</summary>
    public static Funko ToFunko(this UpdateFunkoDto request) => new()
    {
        Nombre = request.Nombre,
        Precio = request.Precio,
        Stock = request.Stock,
        Categoria = request.Categoria,
        Proveedor = request.Proveedor
    };

    /// <summary>Convierte un Funko en el DTO de respuesta.</summary>
    public static FunkoDto ToDto(this Funko funko) => new(
        funko.Id,
        funko.Nombre,
        funko.Precio,
        funko.Stock,
        funko.Categoria,
        funko.Proveedor,
        funko.CreatedAt,
        funko.UpdatedAt);
}

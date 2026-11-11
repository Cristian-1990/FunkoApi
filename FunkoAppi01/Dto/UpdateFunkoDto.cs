using System.ComponentModel.DataAnnotations;

namespace FunkoAppi01.Dto;

/// <summary>
/// DTO con los datos para actualizar por completo un Funko existente.
/// </summary>
/// <param name="Nombre">Nombre del Funko.</param>
/// <param name="Precio">Precio de venta.</param>
/// <param name="Stock">Unidades disponibles.</param>
/// <param name="Categoria">Categoría a la que pertenece.</param>
/// <param name="Proveedor">Proveedor que lo suministra (opcional).</param>
public record UpdateFunkoDto(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    string Nombre,

    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
    decimal Precio,

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    int Stock,

    [Required(ErrorMessage = "La categoría es obligatoria")]
    string Categoria,

    string? Proveedor = null
);

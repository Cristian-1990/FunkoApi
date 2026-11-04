using System.ComponentModel.DataAnnotations;

namespace FunkoAppi01.Dto;

/// <summary>
/// DTO con los datos para actualizar por completo una Categoria existente.
/// </summary>
/// <param name="Nombre">Nombre de la categoría.</param>
public record UpdateCategoriaDto(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    string Nombre
);

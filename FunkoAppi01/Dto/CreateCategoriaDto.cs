using System.ComponentModel.DataAnnotations;

namespace FunkoAppi01.Dto;

/// <summary>
/// DTO con los datos necesarios para crear una Categoria. El Id lo asigna el repositorio.
/// </summary>
/// <param name="Nombre">Nombre de la categoría.</param>
public record CreateCategoriaDto(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    string Nombre
);

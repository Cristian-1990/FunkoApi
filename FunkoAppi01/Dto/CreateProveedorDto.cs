using System.ComponentModel.DataAnnotations;

namespace FunkoAppi01.Dto;

/// <summary>
/// DTO con los datos necesarios para crear un Proveedor. El Id lo asigna el repositorio.
/// </summary>
/// <param name="Nombre">Nombre del proveedor.</param>
/// <param name="Telefono">Teléfono de contacto.</param>
public record CreateProveedorDto(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    string Nombre,

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
    string Telefono
);

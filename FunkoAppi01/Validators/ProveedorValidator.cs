using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Validators;

/// <summary>
/// Valida las reglas de negocio que las Data Annotations no cubren
/// (los espacios en blanco pasan [Required], pero no son un nombre ni un teléfono válido).
/// </summary>
public class ProveedorValidator : IProveedorValidator
{
    public Result<CreateProveedorDto, DomainError> ValidateCreate(CreateProveedorDto request)
    {
        var errores = ComprobarCamposComunes(request.Nombre, request.Telefono);

        if (errores.Count > 0)
            return Result.Failure<CreateProveedorDto, DomainError>(ProveedorErrors.Validation(errores));

        return Result.Success<CreateProveedorDto, DomainError>(request);
    }

    public Result<UpdateProveedorDto, DomainError> ValidateUpdate(UpdateProveedorDto request)
    {
        var errores = ComprobarCamposComunes(request.Nombre, request.Telefono);

        if (errores.Count > 0)
            return Result.Failure<UpdateProveedorDto, DomainError>(ProveedorErrors.Validation(errores));

        return Result.Success<UpdateProveedorDto, DomainError>(request);
    }

    private static List<string> ComprobarCamposComunes(string nombre, string telefono)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
            errores.Add("El nombre no puede estar vacío ni ser solo espacios");

        if (string.IsNullOrWhiteSpace(telefono))
            errores.Add("El teléfono no puede estar vacío ni ser solo espacios");

        return errores;
    }
}

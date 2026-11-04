using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Validators;

/// <summary>
/// Valida las reglas de negocio que las Data Annotations no cubren
/// (los espacios en blanco pasan [Required], pero no son un nombre válido).
/// </summary>
public class CategoriaValidator : ICategoriaValidator
{
    public Result<CreateCategoriaDto, DomainError> ValidateCreate(CreateCategoriaDto request)
    {
        var errores = ComprobarNombre(request.Nombre);

        if (errores.Count > 0)
            return Result.Failure<CreateCategoriaDto, DomainError>(CategoriaErrors.Validation(errores));

        return Result.Success<CreateCategoriaDto, DomainError>(request);
    }

    public Result<UpdateCategoriaDto, DomainError> ValidateUpdate(UpdateCategoriaDto request)
    {
        var errores = ComprobarNombre(request.Nombre);

        if (errores.Count > 0)
            return Result.Failure<UpdateCategoriaDto, DomainError>(CategoriaErrors.Validation(errores));

        return Result.Success<UpdateCategoriaDto, DomainError>(request);
    }

    private static List<string> ComprobarNombre(string nombre)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
            errores.Add("El nombre no puede estar vacío ni ser solo espacios");

        return errores;
    }
}

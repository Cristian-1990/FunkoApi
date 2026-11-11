using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Validators;

/// <summary>
/// Valida las reglas de negocio que las Data Annotations no cubren
/// (los espacios en blanco pasan [Required], pero no son un nombre válido).
/// </summary>
public class FunkoValidator : IFunkoValidator
{
    public Result<CreateFunkoDto, DomainError> ValidateCreate(CreateFunkoDto request)
    {
        var errores = ComprobarCamposComunes(request.Nombre, request.Categoria, request.Proveedor);

        if (errores.Count > 0)
            return Result.Failure<CreateFunkoDto, DomainError>(FunkoErrors.Validation(errores));

        return Result.Success<CreateFunkoDto, DomainError>(request);
    }

    public Result<UpdateFunkoDto, DomainError> ValidateUpdate(UpdateFunkoDto request)
    {
        var errores = ComprobarCamposComunes(request.Nombre, request.Categoria, request.Proveedor);

        if (errores.Count > 0)
            return Result.Failure<UpdateFunkoDto, DomainError>(FunkoErrors.Validation(errores));

        return Result.Success<UpdateFunkoDto, DomainError>(request);
    }

    /// <summary>Reglas que comparten la creación y la actualización.</summary>
    /// <param name="nombre">Nombre del Funko.</param>
    /// <param name="categoria">Categoría del Funko.</param>
    /// <param name="proveedor">Proveedor del Funko (opcional: null es válido, espacios en blanco no).</param>
    /// <returns>Lista con los errores encontrados; vacía si todo es correcto.</returns>
    private static List<string> ComprobarCamposComunes(string nombre, string categoria, string? proveedor)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
            errores.Add("El nombre no puede estar vacío ni ser solo espacios");

        if (string.IsNullOrWhiteSpace(categoria))
            errores.Add("La categoría no puede estar vacía ni ser solo espacios");

        if (proveedor is not null && string.IsNullOrWhiteSpace(proveedor))
            errores.Add("El proveedor no puede ser solo espacios (déjalo vacío si no aplica)");

        return errores;
    }
}

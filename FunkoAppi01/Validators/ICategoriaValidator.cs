using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Validators;

/// <summary>
/// Validador de dominio de las peticiones de Categoria.
/// </summary>
public interface ICategoriaValidator
{
    Result<CreateCategoriaDto, DomainError> ValidateCreate(CreateCategoriaDto request);
    Result<UpdateCategoriaDto, DomainError> ValidateUpdate(UpdateCategoriaDto request);
}

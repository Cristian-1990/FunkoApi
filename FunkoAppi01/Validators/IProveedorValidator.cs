using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Validators;

/// <summary>
/// Validador de dominio de las peticiones de Proveedor.
/// </summary>
public interface IProveedorValidator
{
    Result<CreateProveedorDto, DomainError> ValidateCreate(CreateProveedorDto request);
    Result<UpdateProveedorDto, DomainError> ValidateUpdate(UpdateProveedorDto request);
}
